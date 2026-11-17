# Contribuir

O desenvolvimento e os testes completos exigem Windows e o SDK definido em `global.json`. A proteção de credenciais usa DPAPI, por isso a suíte completa não é portátil para Linux ou macOS.

Na raiz do clone, execute:

```powershell
dotnet restore C4K.slnx --locked-mode
dotnet build C4K.slnx -c Release --no-restore
dotnet run --project tests/C4K.LibraryTests -c Release --no-build
powershell -NoProfile -ExecutionPolicy Bypass -File scripts/Smoke-Test.ps1
```

Os testes usam diretórios temporários e APIs simuladas. Não configure testes com credenciais reais, torrents privados ou a biblioteca pessoal. Os contratos HTTP não comprovam a reprodução real na TV.

Descreva o problema, a mudança e como verificou o comportamento ao abrir um pull request. Preserve os dados e links de instalações existentes e evite alterações sem relação com a proposta. Mantenha arquivos de texto em UTF-8.

O workflow em `.github/workflows/ci.yml` compila, executa as verificações e valida a publicação self-contained em Windows.
