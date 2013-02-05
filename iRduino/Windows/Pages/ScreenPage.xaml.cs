﻿//========================================//
// iRduino - Created by Mark Silverwood  //
//======================================//

namespace iRduino
{
    using ArduinoInterfaces;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using iRduino.Classes;

    /// <summary>
    ///     Interaction logic for ScreenPage.xaml
    /// </summary>
    public partial class ScreenPage
    {
        public DisplayUnitConfiguration Temp;
        private bool firstLoad = true;
        private List<ComboBox> scv;

        private int screen;

        public ScreenPage()
        {
            InitializeComponent();
        }

        private void StartLoading()
        {
            this.firstLoad = true;
            Temp = (DisplayUnitConfiguration) DataContext;
            this.scv = new List<ComboBox>
                {
                    ScreenVariable1CBox,
                    ScreenVariable2CBox,
                    ScreenVariable3CBox,
                    ScreenVariable4CBox,
                    ScreenVariable5CBox,
                    ScreenVariable6CBox
                };
            foreach (var variable in Temp.HostApp.DisplayMngr.Dictionarys.DisplayVariables)
            {
                foreach (ComboBox comboBox in this.scv)
                {
                    comboBox.Items.Add(variable.Value.Name);
                }
            }
            foreach (ComboBox cbox in this.scv)
            {
                cbox.SelectedIndex = -1;
                cbox.IsEnabled = false;
            }
            this.scv[0].IsEnabled = true;
            screen = Temp.ScreenToEdit;
            if (Temp.Screens[screen].Variables.Count > 0)
            {
                for (int i = 0; i < Temp.Screens[screen].Variables.Count; i++)
                {
                    this.scv[i].IsEnabled = true;
                    DisplayVarsEnum temp222;
                    if (Enum.TryParse(Temp.Screens[screen].Variables[i],
                                      out temp222))
                    {
                        this.scv[i].SelectedItem = Temp.HostApp.DisplayMngr.Dictionarys.DisplayVariables[temp222].Name;
                    }
                    else
                    {
                        this.scv[i].SelectedIndex = -1;
                    }
                }
            }
            else
            {
                this.scv[0].IsEnabled = true;
                this.scv[0].SelectedIndex = -1;
            }
            this.UseCustomHeaderCheck.IsChecked = Temp.Screens[screen].UseCustomHeader;
            this.HeaderTextBox.Text = Temp.Screens[screen].CustomHeader;
            this.firstLoad = false;
        }

        private void ScreenVariablesUpdated(object sender, SelectionChangedEventArgs e)
        {
            ScreenVariablesUpdatedExtracted(Constants.MaxDisplayLengthTM1638, "{0} / 8", 6, this.scv, Temp, SpaceUsedLabel, this.firstLoad);
        }

        protected void ScreenVariablesUpdatedExtracted(int maxDisplayLengthTM1638, string format, int param, List<ComboBox> scvIn, DisplayUnitConfiguration temp, Label spaceUsedLabel, bool firstLoadIn)
        {
            int count = 0;
            int i = 0;
            int stop = 0;
            while (count < maxDisplayLengthTM1638 && i < scvIn.Count)
            {
                stop = i + 1;
                if (scvIn[i].SelectedIndex >= 0)
                {
                    count += temp.HostApp.DisplayMngr.Dictionarys.DisplayVariables.Where(item => item.Value.Name == scvIn[i].SelectedItem.ToString()).Sum(item => item.Value.Length);
                    spaceUsedLabel.Content = String.Format(format, count);
                    if (count > maxDisplayLengthTM1638)
                    {
                        stop = i;
                        i = scvIn.Count;
                    }
                    i++;
                }
                else
                {
                    scvIn[i].IsEnabled = true;
                    scvIn[i].Items.Clear();
                    foreach (var variable in temp.HostApp.DisplayMngr.Dictionarys.DisplayVariables)
                    {
                        if (variable.Value.Length <= maxDisplayLengthTM1638 - count)
                        {
                            scvIn[i].Items.Add(variable.Value.Name);
                        }
                    }
                    i = scvIn.Count;
                }
            }
            for (int j = stop; j < scvIn.Count; j++)
            {
                scvIn[j].Items.Clear();
                scvIn[j].SelectedIndex = -1;
                scvIn[j].IsEnabled = false;
            }
            //save to config
            if (!firstLoadIn)
            {
                for (int p = 0; p < param; p++)
                {
                    if (scvIn[p].SelectedValue != null)
                    {
                        string tempV = scvIn[p].SelectedValue.ToString();
                        var temp2 = new DisplayVarsEnum();
                        bool found = false;
                        foreach (var disVar in temp.HostApp.DisplayMngr.Dictionarys.DisplayVariables)
                        {
                            if (disVar.Value.Name == tempV)
                            {
                                temp2 = disVar.Key;
                                found = true;
                            }
                        }
                        if (found)
                        {
                            if (temp.Screens[temp.ScreenToEdit].Variables.Count - 1 < p)
                            {
                                temp.Screens[temp.ScreenToEdit].Variables.Add("");
                            }
                            temp.Screens[temp.ScreenToEdit].Variables[p] = temp2.ToString();
                        }
                    }
                }
            }
        }

        private void PageDataContextChanged1(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext != null)
            {
                StartLoading();
            }
            else
            {
                MessageBox.Show("Error Loading... Please Try Again.");
            }
        }

        private void ClearButtonClick(object sender, RoutedEventArgs e)
        {
            foreach (var cb in scv)
            {
                cb.SelectedValue = "Space";
                cb.SelectedIndex = -1;
            }
        }

        private void UseCustomHeaderCheckUnchecked(object sender, RoutedEventArgs e)
        {
            HeaderTextBox.Text = "";
            HeaderTextBox.IsEnabled = false;
            Temp.Screens[screen].UseCustomHeader = false;
        }

        private void UseCustomHeaderCheckChecked(object sender, RoutedEventArgs e)
        {
            HeaderTextBox.IsEnabled = true;
            Temp.Screens[screen].UseCustomHeader = true;
        }

        private void HeaderTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            Temp.Screens[screen].CustomHeader = HeaderTextBox.Text;
        }
    }
}