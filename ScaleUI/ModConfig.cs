﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ScaleUI
{
    [ConfigurationPath("ScaleUIConfig.xml")]
    public class ModConfig
    {
        public bool ConfigUpdated { get; set; }
        public float scale { get; set; } = 0;
        private static ModConfig instance;

        public static ModConfig Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Configuration<ModConfig>.Load();
                }

                return instance;
            }
        }

        public void Save()
        {
            Configuration<ModConfig>.Save();
            ConfigUpdated = true;
        }
    }
}