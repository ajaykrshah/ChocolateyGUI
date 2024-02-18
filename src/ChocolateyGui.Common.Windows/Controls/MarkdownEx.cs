// *******************************************
// Copyright 2024 Ivanti. All rights reserved.
// *******************************************

namespace ChocolateyGui.Common.Windows.Controls
{
	using System;
	using System.IO;
	using System.Windows;
	using Markdig.Wpf;

	public static class MarkdownEx
	{
		#region Fields and constants

		public static readonly DependencyProperty UrlProperty = DependencyProperty.RegisterAttached(
			"Url",
			typeof(String),
			typeof(MarkdownViewer),
			new PropertyMetadata(default(String), MarkdownEx.PropertyChangedCallback));

		#endregion

		#region Public interface

		public static String GetUrl(DependencyObject element)
		{
			return (String)element.GetValue(MarkdownEx.UrlProperty);
		}

		public static void SetUrl(DependencyObject element, String value)
		{
			element.SetValue(MarkdownEx.UrlProperty, value);
		}

		#endregion

		#region Private implementation

		private static void PropertyChangedCallback(
			DependencyObject dependencyObject,
			DependencyPropertyChangedEventArgs e)
		{
			var viewer = (MarkdownViewer)dependencyObject;
			var url = e.NewValue as String;
			if (String.IsNullOrWhiteSpace(url))
			{
				viewer.Markdown = null;
				return;
			}

			Uri uri;
			if (!url.StartsWith("pack:") || !Uri.TryCreate(url, UriKind.Absolute, out uri))
			{
				viewer.Markdown = null;
				return;
			}

			var resource = Application.GetResourceStream(uri);
			if (resource == null)
			{
				viewer.Markdown = null;
				return;
			}

			using (var reader = new StreamReader(resource.Stream))
			{
				viewer.Markdown = reader.ReadToEnd();
			}
		}

		#endregion
	}
}