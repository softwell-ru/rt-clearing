# SoftWell.RtClearing.Moex

Отправка ордеров на клиринг в Московскую Биржу через протокол FIX.


## [MoexClearingService](./MoexClearingService.cs)

Основной класс. По входящему Document формирует NewOrderSingle и отправляет его на клиринг.
Также подписывается на все сообщения типа ExecutionReport от биржи и пишет в лог результаты.


## Использование

```c#
SessionSettings sessionSettings = GetMoexSessionSettings();
MoexClearingOptions options = GetOptions();
services.AddMoexClearingService(options, sessionSettings);
```

или

```c#
SessionSettings sessionSettings = GetMoexSessionSettings();
MoexClearingOptions options = GetOptions();
services.AddDefaultClearingRouting(
    opts => opts.AddNamedMoexClearingService("MOEX", options, sessionSettings));
```

## Конфигурация

Для работы сервиса требуется объект [MoexClearingOptions](./Configuration/MoexClearingOptions.cs).
Пока что в нем надо указать только номер счета, который будет уходить по FIX в ордере.


## Docs

http://ftp.moex.com/pub/FIX/ASTS/FIX_FX_OTC/