# SoftWell.RtClearing.Abstractions

## Абстракции и общий функционал отправки ордера на клиринг.

### [ClearingHostedService](./ClearingHostedService.cs)

Ордера читаются из [IDocumentsSource](./IDocumentsSource.cs), с помощью [IClearingMetaExtractor](./IClearingMetaExtractor.cs) проверяются на неоходимость отправки на клиринг и при положительном результате отправляются в [IClearingService](./IClearingService.cs).


### Clearing meta extraction

Используется для удобства получения некоторых параметров ордера (например, tradeId). 

На клиринг передается уже мета-информация об ордере, которая включает исходный документ.


### Использование

```c#
services
    .AddSingleton<IDocumentsSource, MyDocumentSource>()
    .AddClearingService<MyClearingService>()
    .AddDefaultClearingMetaExtractor()
    .AddClearingProcessing();
```


## Отправка ордеров на клиринг в разные клиринговые сервисы

Для данного функционала включается т.н. роутинг. Клиринговые сервисы регистрируются каждый со своим уникальным именем, плюс регистрируется [IClearingServiceNameProvider](./Routing/IClearingServiceNameProvider.cs), который должен для каждого ордера вернуть имя нужного клирингового сервиса. По умолчанию это имя определяется по PartyId клиринговой организации.

[По умолчанию](./Routing/DefaultClearingRouter.cs) один ордер будет обрабатываться одним клиринговым сервисом, но абстракция поддерживает набор сервисов.


### Использование

```c#
services
    .AddSingleton<IDocumentsSource, MyDocumentSource>()
    .AddDefaultClearingRouting(
        opts => opts.AddNamedClearingService<MyClearingService1>("name1")
            .AddNamedClearingService<MyClearingService2>("name2")
            .AddNamedClearingService<MyClearingService3>("name3")
            // опционально. По умолчанию регистрируется PartyIdClearingServiceNameProvider
            .AddClearingServiceNameProvider<MyClearingServiceNameProvider>())
    .AddDefaultClearingMetaExtractor()
    .AddClearingProcessing();
```