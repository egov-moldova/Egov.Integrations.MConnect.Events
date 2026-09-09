# Changelog

All notable changes to this package are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

Release numbers are stamped by CI as part of the publish workflow, so a release's
number is assigned when that workflow runs. Work that has not been published yet
sits under **Unreleased**.

This file was introduced at version 10.0.6. Changes made before that point are not listed here —
see the commit history for those.

## [Unreleased]

### Changed
- Updated `xunit.v3` to 4.0.0 and `xunit.runner.visualstudio` to 4.0.0. xunit.v3 v4
  moves to Microsoft.Testing.Platform and drops VSTest support on the .NET 10 SDK, so
  the CI test step now runs the test app with `dotnet run --project` instead of
  `dotnet test`. This affects the build only — the shipped package is unchanged.
- The publish job is gated on the workflow event rather than the branch, so pushing
  code can never release a package.
