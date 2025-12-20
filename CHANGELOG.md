# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [145.0.7578.0] - 2025-12-20

### Changed
- **Upgraded PDFium from 143.0.7469.0 to 145.0.7578.0** (chromium/7578)
  - Includes significantly improved text reading capabilities
  - Better handling of complex PDF text extraction
  - Enhanced support for special characters and multilingual text
- Updated target framework from .NET Standard 2.1 to .NET 8.0+
  - Removed support for older frameworks (netstandard2.1, net5.0, net6.0)
  - Simplified package dependencies
- Updated Clang toolchain from 14.0.0 to 18.0
- Updated build tools to .NET 9.0

### Removed
- Removed `System.Runtime.CompilerServices.Unsafe` package dependency (no longer needed in .NET 8.0)
- Removed support for older .NET target frameworks

## [143.0.7469.0] - Previous Release

### Note
- Previous releases were not documented in this CHANGELOG
- Version history available through git commit history
