
using System.Collections.Generic;
using System.Data;
using System;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Xml;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using System.Text;
using System.Collections;

public sealed class ConfigFile
{
    // -----------------------------------------------------------------------------
    // ------------клас для зчитування і запису config файла програми---------------
    // -----------------------------------------------------------------------------

    #region LoadFile Section -------------------------------------------------------

    private string configFileName = null;
    private XmlDocument xmlDoc = new XmlDocument();

    public ConfigFile(string parConfigFileName)
    {
        if (string.IsNullOrEmpty(parConfigFileName))
            parConfigFileName = System.IO.Path.GetDirectoryName(
                 System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) +
                 System.IO.Path.DirectorySeparatorChar +
                 System.IO.Path.GetFileNameWithoutExtension(
                 System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase) + ".config";

        this.configFileName = parConfigFileName;
        // Проверим, есть ли такой файл - если нет, создадим
        if (!File.Exists(configFileName))
        {
            string strFile = @"<configuration>
									<appSettings>
                                        <add key=""ServiceUrl"" value=""http://1CSRV/utppsu/ws/ws1.1cws"" />
                                        <add key=""CodeShop"" value=""000000057"" />                                        
                                        <add key=""TimeOut"" value=""30"" />                                        
									</appSettings>
								</configuration>";

            StreamWriter sw = null;
            try
            {
                sw = File.CreateText(configFileName);
                sw.Write(strFile);
            }
            finally
            {
                if (sw != null)
                    sw.Close();
            }
        }

        this.xmlDoc.Load(this.configFileName);

    }
    /*public ConfigFile()
    {			      
		  

      this.ConfigFile(null);
		  
    }*/

    #endregion ---------------------------------------------------------------------

    #region Config Section ---------------------------------------------------------

    public void AddConfigSection(string sectionName, string handlerClass)
    {
        XmlNode rootNode = this.xmlDoc.GetElementsByTagName("configuration").Item(0);

        // Create the configSections node if it doesn't exist as config sections need an entry in this node.
        XmlNode node = this.xmlDoc.DocumentElement["configSections"];

        if (node == null)
        {
            node = this.xmlDoc.CreateElement("configSections");
            if (rootNode.ChildNodes.Count > 0)
            {
                XmlNode firstChild = rootNode.FirstChild;
                rootNode.InsertBefore(node, firstChild);
            }
            else
            {
                rootNode.AppendChild(node);
            }
        }

        // Add the section into the configSections node
        XmlNode subNode = this.xmlDoc.CreateElement("section");
        subNode.Attributes.Append(this.xmlDoc.CreateAttribute("name")).Value = sectionName;
        subNode.Attributes.Append(this.xmlDoc.CreateAttribute("type")).Value = handlerClass;
        node.AppendChild(subNode);

        // Now create the actual section if it's not there.
        node = this.xmlDoc.DocumentElement[sectionName];
        if (node == null)
        {
            node = this.xmlDoc.CreateElement(sectionName);
            rootNode.AppendChild(node);
        }

        // Save the config file.
        this.xmlDoc.Save(this.configFileName);
    }

    // Check whether a configuration section exists
    private bool ConfigSectionExists(string sectionName)
    {
        return (this.xmlDoc.DocumentElement[sectionName] != null);
    }

    // Set an attribute in the configuration file
    public void SetConfigAttribute(string sectionName, string attributeName, string attributeValue, bool createIfNotExist)
    {
        // Get the section node
        XmlNode sectionNode = this.xmlDoc.DocumentElement[sectionName];

        XmlAttribute attr = sectionNode.Attributes[attributeName];

        if (attr == null)
        {
            if (!createIfNotExist)
            {
                // Leave it.
                return;
            }
            else
            {
                // Create the attribute.
                attr = this.xmlDoc.CreateAttribute(attributeName);
                sectionNode.Attributes.Append(attr);
            }
        }
        // Now set its value
        attr.Value = attributeValue;

        // Save the config file.
        this.xmlDoc.Save(this.configFileName);
    }

    public string GetConfigAttribute(string sectionName, string attributeName, bool createIfNotFound)
    {
        XmlNode sectionNode = this.xmlDoc.DocumentElement[sectionName];
        XmlAttribute attr = sectionNode.Attributes[attributeName];

        if (attr == null)
        {
            if (createIfNotFound)
            {
                attr = this.xmlDoc.CreateAttribute(attributeName);
                sectionNode.Attributes.Append(attr);
                this.xmlDoc.Save(this.configFileName);
            }
        }

        return (attr == null) ? "" : attr.Value;
    }

    #endregion // ------------------------------------------------------------------

    #region appSetting Section -----------------------------------------------------

    // Create an application setting
    public void CreateAppSetting(string settingName, string settingValue)
    {
        // Get the setting node, creating if it doesn't exist.
        XmlNode settingNode = GetAppSettingNode(settingName, true);

        // Set its value.
        settingNode.Attributes["value"].Value = settingValue;

        // Save changes
        this.xmlDoc.Save(this.configFileName);
    }

    // Add an application setting, and its value (if provided)
    // Returns created node.
    private XmlNode AddAppSetting(string settingName, string settingValue)
    {
        // Get the appSettings node
        XmlNode appSettingsNode = this.xmlDoc.DocumentElement["appSettings"];

        // Create the key attribute
        XmlAttribute keyAttr = this.xmlDoc.CreateAttribute("key");
        keyAttr.Value = settingName;

        // Set is value
        XmlAttribute valueAttr = this.xmlDoc.CreateAttribute("value");
        valueAttr.Value = (settingValue == null) ? "" : settingValue;

        // Create the node for the setting
        XmlNode childNode = this.xmlDoc.CreateElement("add");
        childNode.Attributes.Append(keyAttr);
        childNode.Attributes.Append(valueAttr);

        // Add this to the appSettings node
        appSettingsNode.AppendChild(childNode);

        // Return the child.
        return childNode;
    }

    public void SetAppSetting(string settingName, string settingValue, bool createIfNotExist)
    {
        // Get the section node
        //XmlNode sectionNode = this.xmlDoc.DocumentElement["appSettings"];

        XmlNode node = GetAppSettingNode(settingName, createIfNotExist);

        if (node != null)
        {
            node.Attributes["value"].Value = settingValue;
            this.xmlDoc.Save(this.configFileName);
        }
    }

    // Returns the node in the appSettings part with the given name.
    // Optional extra to create the node if it is not found.
    private XmlNode GetAppSettingNode(string settingName, bool createIfNotFound)
    {
        // Get the appSettings node
        XmlNode appSettingsNode = this.xmlDoc.DocumentElement["appSettings"];
        XmlNode foundNode = null;

        // Find node corresponding to the setting name
        foreach (XmlNode childNode in appSettingsNode.ChildNodes)
        {
            if (childNode.Attributes["key"].Value == settingName)
            {
                foundNode = childNode;
                break;
            }
        }

        // Did we find it?
        if (foundNode == null)
        {
            // Nope.
            if (createIfNotFound)
            {
                foundNode = AddAppSetting(settingName, null);
            }
        }

        return foundNode;
    }

    // Returns an appSettings value, given its key
    public string GetAppSetting(string settingName)
    {
        XmlNode appSettingNode = GetAppSettingNode(settingName, false);

        return (appSettingNode == null) ? null : appSettingNode.Attributes["value"].Value;
    }

    // видаляє ноду з config файла
    public void RemoveAppSetting(string settingName)
    {
        XmlNode appSettingsNode = this.xmlDoc.DocumentElement["appSettings"];
        XmlNode foundNode = null;

        foreach (XmlNode childNode in appSettingsNode.ChildNodes)
        {
            if (childNode.Attributes["key"].Value == settingName)
            {
                foundNode = childNode;
                break;
            }
        }

        if (foundNode != null)
        {
            appSettingsNode.RemoveChild(foundNode);
            this.xmlDoc.Save(this.configFileName);
        }
    }

    #endregion ---------------------------------------------------------------------

    // -----------------------------------------------------------------------------
}