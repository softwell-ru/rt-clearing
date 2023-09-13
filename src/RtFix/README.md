# SoftWell.RtClearing.RtFix


Адаптер для преобразования FIX-сообщений от RuTerminal в fpml ордеров и репортинга информации по ордерам по FIX.

## FixToDocumentsAdapter\<TFixMessagesReader\>

Реализует [IDocumentsSource](../Abstractions/IDocumentsSource.cs). Читает все сообщения из \<TFixMessagesReader\>, в сообщениях типа TradeCaptureReport ищет SecurityXML, пытается десериализовать его значение в Document и при успехе отправляет полученный Document в результирующий IAsyncEnumerable.

## FixRtTradeReporter\<TFixMessagesSender\>

Реализует [IRtTradeReporter](../Abstractions/IRtTradeReporter.cs). Преобразует TradeStatusChanged в FIX-сообщение и отправляет его в \<TFixMessagesSender\>.

#### Использование

```c#
SessionSettings sessionSettings = GetSessionSettings();
services.AddRtFix(sessionSettings);
```

После чего можно инжектить IDocumentsSource и IRtTradeReporter.