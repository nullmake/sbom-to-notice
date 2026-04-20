# SbomToNotice

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
SbomToNotice <path-to-sbom-file> -o <output-file-path>
```

### Arguments

- `<path-to-sbom-file>`: The file path to your CycloneDX SBOM file (e.g., `bom.json`).

### Options

- `-o, --output <file-path>`: (Optional) The path to the output license notice file. If not specified, the output is printed to standard output.

## License

This project is licensed under the Apache License 2.0. See the [LICENSE.txt](LICENSE.txt) file for details.
