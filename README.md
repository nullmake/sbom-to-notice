# SbomToNotice

[![NuGet Version](https://img.shields.io/nuget/v/SbomToNotice)](https://www.nuget.org/packages/SbomToNotice)
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)

SbomToNotice is a .NET tool that automates the generation of a comprehensive license notice file from a Software Bill of Materials (SBOM). It simplifies compliance by aggregating license information for your project's components.

## Features

- **CycloneDX Support**: Parses CycloneDX formatted SBOMs.
- **Automated License Fetching**: Automatically retrieves license texts from GitHub repositories, SPDX license list, or specified URLs.
- **Easy Integration**: Designed to be used as a .NET tool in CI/CD pipelines.

## Installation

You can install SbomToNotice as a global tool:

```bash
dotnet tool install -g SbomToNotice
```

## Usage

To generate a license notice file, use the following command:

```bash
SbomToNotice manifest.cyclonedx.json -o ThirdPartyNotices.html --ofmt Html
```

```bash
SbomToNotice manifest.cyclonedx.json -o ThirdPartyNotices.md --ofmt Markdown
```

### Arguments

- `<file>`:
  Path to the SBOM file for generating the license notice.

### Options

- `-o, --output <output>`:
  File path for outputting the license notice.

- `--ofmt, --output-format <Html|Markdown>`:
  File format for outputting the license notice. [default: Markdown]

- `--refresh-cache`:
  Forces downloading and overwrites the local cache for components in the specified SBOM.

- `-?, -h, --help`:
  Show help and usage information

- `--version`:
  Show version information

## License

This project is licensed under the Apache License 2.0. See the [LICENSE.txt](LICENSE.txt) file for details.
