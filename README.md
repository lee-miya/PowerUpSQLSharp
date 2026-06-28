# PowerUpSQLSharp

C# rewrite of [NetSPI/PowerUpSQL](https://github.com/NetSPI/PowerUpSQL) with domain-driven architecture, PowerUpSQL-compatible CLI names, standalone execution, and Sliver `execute-assembly --in-process` support.

## Requirements

- .NET Framework 4.8
- Visual Studio 2022 or MSBuild 17+

## Build

```powershell
.\scripts\release.ps1
```

Output: `dist\PowerUpSQLSharp.exe`

## Usage

```powershell
PowerUpSQLSharp.exe --help
PowerUpSQLSharp.exe Get-SQLInstanceLocal
PowerUpSQLSharp.exe Get-SQLInstanceLocal --help
```

## Documentation

- [PowerUpSQLSharp_Requirements_v2.0.md](PowerUpSQLSharp_Requirements_v2.0.md)
- [docs/PowerUpSQL-Mapping.md](docs/PowerUpSQL-Mapping.md)
- [docs/Acceptance-Tests.md](docs/Acceptance-Tests.md)

## License

BSD-3-Clause (see PowerUpSQL upstream)
