# ecoAPM Statiq Pipelines

Pipelines and helpers used in ecoAPM's static sites

[![Install](https://img.shields.io/nuget/v/ecoAPM.StatiqPipelines?logo=nuget&label=Install)](https://www.nuget.org/packages/ecoAPM.StatiqPipelines/)
[![CI](https://github.com/ecoAPM/StatiqPipelines/actions/workflows/CI.yml/badge.svg)](https://github.com/ecoAPM/StatiqPipelines/actions/workflows/CI.yml)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_StatiqPipelines&metric=coverage)](https://sonarcloud.io/summary/new_code?id=ecoAPM_StatiqPipelines)

[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_StatiqPipelines&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=ecoAPM_StatiqPipelines)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_StatiqPipelines&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=ecoAPM_StatiqPipelines)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=ecoAPM_StatiqPipelines&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=ecoAPM_StatiqPipelines)

## Requirements

- .NET SDK 6

## Installation

```bash
dotnet add {project} package ecoAPM.StatiqPipelines
```

## Usage

This package currently contains one pipeline and one module.

### CopyFromNPM

This pipeline copies files from the `node_modules` directory to a set location in your output.

```c#
bootstrapper.AddPipeline("NPM", new CopyFromNPM(new [] {
	"bootstrap/dist/css/bootstrap.min.css",
	"bootstrap/dist/js/bootstrap.min.js",
	"jquery/dist/jquery.min.js",
	"marked/marked.min.js",
	"notosans/*",
	"vue/dist/vue.global.prod.js"
});
```

### NiceURL

This module can be added to your Content pipeline to improve the output URL format.

So, `input/category/page.md => http://localhost/directory/file`
instead of the default `output/category/page.html`

```c#
bootstrapper.ModifyPipeline("Content", p => p.ProcessModules.Add(new NiceURL()));
```

## Contributing

Please be sure to read and follow ecoAPM's [Contribution Guidelines](CONTRIBUTING.md) when submitting issues or pull requests.