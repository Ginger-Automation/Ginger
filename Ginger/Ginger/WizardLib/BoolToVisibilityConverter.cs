#region License
/*
Copyright © 2014-2023 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

using System;
using System.Windows;
using System.Windows.Data;

namespace GingerWPF.WizardLib
{
	/// <summary>
	/// 
	/// </summary>
	public class BoolToVisibilityConverter: IValueConverter
	{
		/// <summary>
		/// Gets or sets the value if false.
		/// </summary>
		public Visibility ValueIfFalse { get; set; }
		/// <summary>
		/// Gets or sets the value if true.
		/// </summary>
		public Visibility ValueIfTrue  { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="BoolToVisibilityConverter"/> class.
		/// </summary>
		public BoolToVisibilityConverter()
		{
			ValueIfTrue = Visibility.Visible;
			ValueIfFalse = Visibility.Hidden;
		}

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value produced by the binding source.</param>
		/// <param name="targetType">The type of the binding target property.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			Visibility result = ((bool)value) ? this.ValueIfTrue : this.ValueIfFalse;
			return result;
		}

		/// <summary>
		/// Converts a value.
		/// </summary>
		/// <param name="value">The value that is produced by the binding target.</param>
		/// <param name="targetType">The type to convert to.</param>
		/// <param name="parameter">The converter parameter to use.</param>
		/// <param name="culture">The culture to use in the converter.</param>
		/// <returns>
		/// A converted value. If the method returns null, the valid null value is used.
		/// </returns>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			bool result = ((Visibility)value == this.ValueIfTrue);
			return result;
		}
	}
}
