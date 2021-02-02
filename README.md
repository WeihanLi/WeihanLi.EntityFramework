# WeihanLi.EntityFramework 

[![WeihanLi.EntityFramework](https://img.shields.io/nuget/v/WeihanLi.EntityFramework.svg)](https://www.nuget.org/packages/WeihanLi.EntityFramework/)

[![WeihanLi.EntityFramework Latest](https://img.shields.io/nuget/vpre/WeihanLi.EntityFramework)](https://www.nuget.org/packages/WeihanLi.EntityFramework/absoluteLatest)

[![Pipeline Build Status](https://weihanli.visualstudio.com/Pipelines/_apis/build/status/WeihanLi.WeihanLi.EntityFramework?branchName=dev)](https://weihanli.visualstudio.com/Pipelines/_build/latest?definitionId=11&branchName=dev)

![Github Build Status](https://github.com/WeihanLi/WeihanLi.EntityFramework/workflows/dotnetcore/badge.svg)

## Intro

EntityFramework extensions

## Package Release Notes

> Package Versions
>
> For EF Core 2.x , use 1.4.x and below
>
> For EF Core 3.x, use 1.5.0 above, and 2.0.0 below
>
> For EF Core 5.x, use 2.0.0 and above

for more package version details, see package release notes details [here](./docs/ReleaseNotes.md)

## Features

- Repository
  
  - `EFRepository`
  - `EFRepositoryGenerator`

- UoW
  
  - `EFUnitOfWork`  

- DbFunctions
  
  - `JsonValue` implement `JSON_VALUE` for SqlServer 2016 and above

- Audit

  - auto audit for entity change

- Extensions

  - Update specific column `Update`
  - Update without specific column `UpdateWithout`

## Contact

contact me <weihanli@outlook.com> if you need
