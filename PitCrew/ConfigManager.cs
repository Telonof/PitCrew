using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using PitCrew.Models;

namespace PitCrew;

internal class ConfigManager
{
    private readonly XDocument properties;

    private string nextTheme;

    private StyleInclude? savedStyle = null;

    private static readonly string fileName = "Properties.xml";

    public ConfigManager()
    {
        //Test for XML and create if not found.
        if (!File.Exists(fileName))
        {
            var config = new XElement("Root",
                new XComment("If you don't know what you are doing, avoid touching this file!"),
                new XElement("Instances"),
                new XElement("Settings",
                    new XElement("Setting",
                        new XAttribute("Name", "Lang"),
                        new XAttribute("Value", "English")
                    ),
                    new XElement("Setting",
                        new XAttribute("Name", "Theme"),
                        new XAttribute("Value", "Dark")
                    ),
                    new XElement("Setting",
                        new XAttribute("Name", "LastOpenedPath"),
                        new XAttribute("Value", "")
                    )
                )
            );
            config.Save(fileName);
        }
        properties = XDocument.Load(fileName);
        nextTheme = GetSetting("Theme");
        SwitchTheme();
    }

    public string? GetSetting(string setting)
    {
            return properties.Root?.Element("Settings")?
                            .Elements("Setting")?
                            .FirstOrDefault(at => at.Attribute("Name").Value.Equals(setting, StringComparison.OrdinalIgnoreCase))?
                            .Attribute("Value")?.Value;
    }

    public void SetSetting(string name, string value)
    {
        XElement? setting = properties.Root.Element("Settings")?
                            .Elements("Setting")?
                            .FirstOrDefault(at => at.Attribute("Name").Value.Equals(name));

        if (setting != null)
            setting.SetAttributeValue("Value", value);

        properties.Save(fileName);
    }

    public void SetInstanceName(string oldName, string newName)
    {
        XElement? savedInstance = properties.Root.Element("Instances")?
                            .Elements("Instance")?
                            .FirstOrDefault(at => at.Attribute("Name").Value.Equals(oldName));

        if (savedInstance != null)
            savedInstance.SetAttributeValue("Name", newName);
        properties.Save(fileName);
    }

    public void DeleteInstance(Instance instance)
    {
        XElement? savedInstance = properties.Root.Element("Instances")?
                    .Elements("Instance")?
                    .FirstOrDefault(at => at.Attribute("Name").Value.Equals(instance.Name));


        if (savedInstance != null)
        {
            savedInstance.Remove();
            properties.Save(fileName);
        }
    }

    public void SaveInstance(Instance instance)
    {
        XElement xmlinstance = new XElement("Instance",
                                new XAttribute("Name", instance.Name),
                                new XAttribute("Path", instance.ManifestPath));

        properties.Root.Element("Instances").Add(xmlinstance);
        properties.Save(fileName);
    }

    public List<Instance> LoadInstances()
    {
        List<Instance> instances = [];
        foreach (var instance in properties.Root.Element("Instances").Elements("Instance"))
        {
            string name = instance.Attribute("Name")?.Value;
            string path = instance.Attribute("Path")?.Value;

            instances.Add(new Instance() { Name = name, ManifestPath = path });
        }
        return instances;
    }

    public void SwitchTheme()
    {
        ThemeVariant variant = ThemeVariant.Dark;
        string theme = nextTheme;
        SetSetting("Theme", theme);

        //Add requested theme
        if (nextTheme.Equals("Light", StringComparison.OrdinalIgnoreCase))
        {
            variant = ThemeVariant.Light;
            nextTheme = "Dark";
        } else
        {
            nextTheme = "Light";
        }

        Application.Current.RequestedThemeVariant = variant;

        if (savedStyle != null)
            Application.Current.Styles.Remove(savedStyle);

        Uri uriLoc = new Uri($"avares://PitCrew/Styles/{theme}.axaml");
        StyleInclude style = new StyleInclude(uriLoc) { Source = uriLoc };
        Application.Current.Styles.Add(style);
        savedStyle = style;
    }
}