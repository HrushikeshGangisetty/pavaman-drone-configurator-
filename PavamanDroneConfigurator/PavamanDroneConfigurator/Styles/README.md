# Styles

This folder contains UI styling resources for the Avalonia application.

## Purpose

Store custom styles, themes, and resource dictionaries that define the visual appearance of the application.

## Contents

- **Custom Styles**: XAML files defining custom control styles
- **Theme Resources**: Color schemes, brushes, and visual resources
- **Style Dictionaries**: Merged resource dictionaries for organizing styles

## Usage

Styles defined here are typically merged into `App.axaml` or specific view resources to apply consistent theming across the application.

Example:
```xml
<Application.Styles>
    <StyleInclude Source="/Styles/CustomButtonStyles.axaml"/>
</Application.Styles>
```
