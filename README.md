## Описание
Репозиторий с демонстрационной системой использования ML инструментов для работы с интернет магазинами.

Проект написан на C# .NET 8.0.
## Как использовать

Для проекта требуется установить внешние записимости:
* [Olama](https://ollama.com/download) 

* Загрузить модель [mistral](https://ollama.com/library/mistral-small)

```
> run mistral-small
```

* Загрузить модель [llama3.1](https://ollama.com/library/llama3.1)

```
> run llama3.1
```

Вот [тут](https://dev.to/timesurgelabs/how-to-run-llama-3-locally-with-ollama-and-open-webui-297d) подробнее о процессе установки.

* Далее, поставить себе dotnet sdk [windows](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-8.0.403-windows-x64-installer) или (другие)[https://dotnet.microsoft.com/en-us/download/dotnet/8.0]

* Выполнить сборку и запуск проекта 

```
> dotnet run --project MarketSphere\MarketSphere.csproj
```

* Найти в выводе прокраммы адрес по которому можно осуществить переход на работающее приложение

```
> Now listening on: http://localhost:5178
```

* Вносить инфу о подарках можно в Json по пути:

```
> MarketSphere/wwwroot/data/gifts.json
``` 

* После внесения изменений в данные, желательно рестартовать программу