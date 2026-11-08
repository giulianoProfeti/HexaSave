# HexaSave

Módulo de persistência para Unity organizado segundo a arquitetura hexagonal.

- Plataform: Windows 64-bit
- UnityEditor Version: 6000.3.10f1

## Glosário

Termo | Significado
------|------------
Progresso (PlayerProgress) | Estado salvo de um jogador, com conteúdo opaco e metadados
Conteúdo (SaveData) | Dados do jogo em texto; o módulo não os interpreta
Versão (SaveVersion) | Contador que aumenta a cada gravação; base para detectar conflitos
Pontuação (ScoreEntry) | Fato imutável: jogador, nome público, pontos e instante
Dispositivo (DeviceId) | Identificador do aparelho que fez a última gravação

## Estrutura de pastas

Assets/
├── HexaSave/
│   ├── Domain/                          
│   ├── Application/                     
│   └── Adapters/
│       ├── Platform/                    
│       │   ├── SystemClock.cs
│       │   └── UnityDeviceInfo.cs
│       └── InMemory/
│           ├── InMemorySaveRepository.cs
│           ├── InMemoryAuthService.cs
│           └── InMemoryLeaderboardService.cs
└── DemoGame/
    ├── Composition/
    │   ├── SaveProvider.cs
    │   ├── HexaSaveSettings.cs
    │   ├── HexaSaveModule.cs
    │   └── GameBootstrapper.cs
    ├── Gameplay/
    │   └── HexaSaveSmokeTest.cs
    ├── Settings/                        
    └── Scenes/
        └── Main.unity