namespace SaaS.PowerBnB.Modules.Charging.Domain.Enums;

public enum ConnectorType
{
    Type2 = 1,   // Padrão Europeu/Maioria no Brasil
    GB_T = 2,    // Padrão Chinês (BYD antigos)
    CCS2 = 3,    // Carga Rápida DC
    WallPlug = 4 // Tomada comum 20A (Para motos/patinetes)
}

public enum ChargerStatus
{
    Offline = 0,      // Disjuntor sem internet/desligado
    Available = 1,    // Online e pronto para uso
    Occupied = 2,     // Carro plugado e carregando
    Maintenance = 3   // Anfitrião bloqueou para manutenção
}
