# WeihanLi.EntityFramework [![WeihanLi.EntityFramework](https://img.shields.io/nuget/v/WeihanLi.EntityFramework.svg)](https://www.nuget.org/packages/WeihanLi.EntityFramework/)

[![Pipeline Build Status](https://weihanli.visualstudio.com/Pipelines/_apis/build/status/WeihanLi.WeihanLi.EntityFramework?branchName=dev)](https://weihanli.visualstudio.com/Pipelines/_build/latest?definitionId=11&branchName=dev)

![Github Build Status](https://github.com/WeihanLi/WeihanLi.EntityFramework/workflows/dotnetcore/badge.svg)

## Intro

EntityFramework extensions

## Package Release Notes

> Package Versions
>
> For EF Core 2.x , use 1.4.x and below
>
> For EF Core 3.x, use 1.5.0 and above

for more package version details, see package release notes details [here](./docs/ReleaseNotes.md)

## Features

- Repository
  
  - `EFRepository`
  - `EFRepositoryGenerator`

- UoW
  
  - `EFUnitOfWork`  

- DbFunctions
  
  - `JsonValue` implement `JSON_VALUE` in SqlServer 2016 and above

- Audit

  - auto audit for entity change

- Interceptors

  - `QueryWithNoLockerInterceptor`

- Extensions

  - Update specific column
  - Update without specific column

## Contact

contact me <weihanli@outlook.com> if you need
