# Chocolatey GUI

Chocolatey GUI is a user interface for [Chocolatey](http://chocolatey.org) (the Machine Package Manager for Windows).

## Installation

You can install Chocolatey GUI via Chocolatey itself by executing:

```choco install ChocolateyGUI```

## Build Status

| GitHub Action                                                                                                                                                                                                  |
|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [![GitHub Workflow Status (branch)](https://img.shields.io/github/workflow/status/chocolatey/ChocolateyGUI/Build/develop?logo=github)](https://github.com/chocolatey/ChocolateyGUI/actions/workflows/build.yml)  |

## Information

* [Chocolatey Community Repository](https://community.chocolatey.org)
* [Chocolatey Documentation](https://docs.chocolatey.org)
* [Twitter](https://twitter.com/chocolateynuget)

### Documentation

You can find information about Chocolatey GUI here: [https://docs.chocolatey.org/en-us/chocolatey-gui/](https://docs.chocolatey.org/en-us/chocolatey-gui/)

### Requirements

* .NET Framework 4.8
* Should work on all Windows Operating Systems from Windows 7 SP1 and above, and Windows Server 2008 R2 SP1 and above

### License / Credits

Apache 2.0 - see [LICENSE](https://github.com/chocolatey/chocolateygui/blob/develop/LICENSE.txt) and [NOTICE](https://github.com/chocolatey/chocolateygui/blob/develop/NOTICE) files.

## Contributing

If you would like to contribute code or help squash a bug or two, that's awesome. Please familiarize yourself with [CONTRIBUTING](https://github.com/chocolatey/chocolateygui/blob/develop/CONTRIBUTING.md).

### Building

* It is assumed that a version of Visual Studio 2019 or newer is already installed on the machine being used to complete the build.
* `choco install wixtoolset -y`
* **OPTIONAL:** Set `FXCOPDIR` environment variable, which can be set using [vswhere](https://chocolatey.org/packages/vswhere) and the following command:
   ```ps1
   $FXCOPDIR = vswhere -products * -latest -prerelease -find **/FxCopCmd.exe
   [Environment]::SetEnvironmentVariable("FXCOPDIR", $FXCOPDIR, 'User')
   refreshenv
   ```
* Install WiX toolset integration for your Visual Studio Integration from [here](https://marketplace.visualstudio.com/items?itemName=WixToolset.WixToolsetVisualStudio2019Extension)
* From an **Administrative** PowerShell Window, navigate to the folder where you have cloned the Chocolatey GUI repository and run `build.ps1`, this will run Cake and it will go through the build script.
  ```
  ./build.ps1
  ```

### Localization

If you are interested in helping with the effort in translating the various portions of the Chocolatey GUI UI into different languages, you can find out more about using the [transifex](https://www.transifex.com/) service in this [how to article](https://docs.chocolatey.org/en-us/chocolatey-gui/localization).

## Committers

Committers, you should be very familiar with [COMMITTERS](https://github.com/chocolatey/chocolateygui/blob/develop/COMMITTERS.md).

## Features:

* View all **installed** and **available** packages
* **Update** installed but outdated packages
* **Install** and **uninstall** packages
* See detailed **package information**

![Chocolatey GUI](https://github.com/chocolatey/ChocolateyGUI/blob/10809890189206cece4b64ab038f33d11cf7b840/docs/Screenshots/Application_Loaded.png)

## Credits

Chocolatey GUI is brought to you by quite a few people and frameworks. See [CREDITS](https://github.com/chocolatey/chocolateygui/blob/develop/CREDITS.md) (just CREDITS.md in the zip folder)
