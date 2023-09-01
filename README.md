# WeihanLi.EntityFramework

[![WeihanLi.EntityFramework](https://img.shields.io/nuget/v/WeihanLi.EntityFramework.svg)](https://www.nuget.org/packages/WeihanLi.EntityFramework/)

[![WeihanLi.EntityFramework Latest](https://img.shields.io/nuget/vpre/WeihanLi.EntityFramework)](https://www.nuget.org/packages/WeihanLi.EntityFramework/absoluteLatest)

[![Pipeline Build Status](https://weihanli.visualstudio.com/Pipelines/_apis/build/status/WeihanLi.WeihanLi.EntityFramework?branchName=dev)](https://weihanli.visualstudio.com/Pipelines/_build/latest?definitionId=11&branchName=dev)

![Github Build Status](https://github.com/WeihanLi/WeihanLi.EntityFramework/workflows/default/badge.svg)

## Intro

EntityFramework extensions

## Package Release Notes

> Package Versions
>
> For EF Core 2.x , use 1.4.x and below
>
> For EF Core 3.x, use 1.5.0 above, and 2.0.0 below
>
> For EF Core 5/6, use 2.x
>
> For EF 7, use 3.0.0 and above
>
> For EF 8 and above, use 8.x or above major-version match versions

## Features

- Repository
  
  - `EFRepository`
  - `EFRepositoryGenerator`

- UoW
  
  - `EFUnitOfWork`  

- DbFunctions
  
  - `JsonValue` implement `JSON_VALUE` for SqlServer 2016 and above

- Audit

  - Auto auditing for entity changes

- Extensions

  - Update specific column(s) `Update`
  - Update without specific column(s) `UpdateWithout`

## Contact

contact me <weihanli@outlook.com> if you need
