﻿using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using StockAudit;
using StockAudit.Droid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(ScanEntryControl), typeof(ScanEntryRenderer))]
namespace StockAudit.Droid
{
    public class ScanEntryRenderer : EntryRenderer
    {
        protected override void OnElementChanged(ElementChangedEventArgs<Entry> e)
        {
            base.OnElementChanged(e);
            Control?.SetBackgroundColor(Android.Graphics.Color.Transparent);
            if (Control != null)
            {
                Control.Click += (sender, evt) =>
                {
                    new Handler().Post(delegate
                    {
                        try
                        {
                            var imm = (InputMethodManager)Control.Context.GetSystemService(Android.Content.Context.InputMethodService);
                            var result = imm.HideSoftInputFromWindow(Control.WindowToken, 0);
                        }
                        catch (Exception Ex) 
                        {

                        }
                    });
                };
                Control.FocusChange += (sender, evt) =>
                {
                    if (Globals.CrateEnabled == true)
                        return;

                    if (Globals.MNUAL_KEYBOARD == false)
                    {
                        e.NewElement.Focus();
                        new Handler().Post(delegate
                        {
                            try
                            {

                                var imm = (InputMethodManager)Control.Context.GetSystemService(Android.Content.Context.InputMethodService);
                                var result = imm.HideSoftInputFromWindow(Control.WindowToken, 0);

                            }
                            catch (Exception)
                            {

                            }
                        });
                    }

                };
            }
            if (e.NewElement != null)
            {
                ((ScanEntryControl)e.NewElement).PropertyChanging += OnPropertyChanging;
            }

            if (e.OldElement != null)
            {
                ((ScanEntryControl)e.OldElement).PropertyChanging -= OnPropertyChanging;
            }
            // Disable the Keyboard on Focus
            this.Control.ShowSoftInputOnFocus = false;

        }

        private void OnPropertyChanging(object sender, PropertyChangingEventArgs propertyChangingEventArgs)
        {
            if (Globals.CrateEnabled == true)
                return;
            // Check if the view is about to get Focus
            if (propertyChangingEventArgs.PropertyName == VisualElement.IsFocusedProperty.PropertyName)
            {

                if (Globals.MNUAL_KEYBOARD == false)
                {
                    // Dismiss the Keyboard 
                    InputMethodManager imm = (InputMethodManager)this.Context.GetSystemService(Context.InputMethodService);
                    imm.HideSoftInputFromWindow(this.Control.WindowToken, HideSoftInputFlags.None);
                }
            }
        }
    }
}