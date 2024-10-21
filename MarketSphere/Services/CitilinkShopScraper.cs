using System.Text;
using MarketSphere.Models;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Globalization;

namespace MarketSphere.Services;

public class CitilinkShopScraper
{
    private readonly ILogger<CitilinkShopScraper> _logger;

    public CitilinkShopScraper(ILogger<CitilinkShopScraper> logger)
    {
        _logger = logger;
    }

    public async Task<IReadOnlyCollection<ProductItem>> Search(string query, int? maxRubles)
    {
        var options = new ChromeOptions
        {

        };
        //options.AddAdditionalChromeOption("excludeSwitches", new[]{ "enable-automation" });
        //options.AddAdditionalChromeOption("--incognito", false);
        //options.AddExcludedArguments(new List<string>() { "enable-automation" });
        using var driver = new ChromeDriver(options);

        /*driver.ExecuteCdpCommand("Page.addScriptToEvaluateOnNewDocument", new Dictionary<string, object>()
        {
            {"source","const defaultGetter = Object.getOwnPropertyDescriptor(\n      Navigator.prototype,\n      \"webdriver\"\n    ).get;\n    defaultGetter.apply(navigator);\n    defaultGetter.toString();\n    Object.defineProperty(Navigator.prototype, \"webdriver\", {\n      set: undefined,\n      enumerable: true,\n      configurable: true,\n      get: new Proxy(defaultGetter, {\n        apply: (target, thisArg, args) => {\n          Reflect.apply(target, thisArg, args);\n          return false;\n        },\n      }),\n    });\n    const patchedGetter = Object.getOwnPropertyDescriptor(\n      Navigator.prototype,\n      \"webdriver\"\n    ).get;\n    patchedGetter.apply(navigator);\n    patchedGetter.toString();"}
            //{"source","\"const newProto = navigator.__proto__;delete newProto.webdriver; navigator.__proto__ = newProto\""}
            //{"source","Object.defineProperty(navigator, 'webdriver', { get: () => undefined }); console.log(navigator.webdriver);"}
        });*/
        //hasOwnProperty.call(navigator, 'webdriver')
        //await driver.Navigate().GoToUrlAsync("https://bot.sannysoft.com/");

        var queryBuilder = new StringBuilder();

        queryBuilder.Append($"https://www.citilink.ru/search/?text={query}");

        if (maxRubles is not null)
        {
            queryBuilder.Append($"&r=price_filter_group_id:0-{maxRubles.Value.ToString(CultureInfo.InvariantCulture)}");
        }

        var url = queryBuilder.ToString();

        _logger.LogInformation("Url: {Url}", url);

        await driver.Navigate().GoToUrlAsync(url);
        await Task.Delay(TimeSpan.FromSeconds(7));

        var elements = driver.FindElements(By.CssSelector("div[data-meta-name='ProductVerticalSnippet']"));

        var list = new List<ProductItem>();
        foreach (IWebElement? element in elements)
        {
            var item = new ProductItem();
            var img = element.FindElement(By.CssSelector("img"));
            if (img is not null)
            {
                item.Img = img.GetAttribute("src");
            }

            var prices = element.FindElements(By.CssSelector("span[data-meta-name='Snippet__price'] span"));

            if (prices?.Any() == true)
            {
                item.Price = prices[0].GetAttribute("data-meta-price");
            }

            var titles = element.FindElements(By.CssSelector("a[data-meta-name='Snippet__title']"));
            if (titles?.Any() == true)
            {
                item.Url = titles[0].GetAttribute("href");
                item.Name = titles[0].Text;
            }

            var comments = element.FindElements(By.CssSelector("div[data-meta-name='MetaInfo_opinionsCount']"));

            item.HasComments = comments?.Any();

            if (!string.IsNullOrEmpty(item.Price))
            {
                list.Add(item);
            }
        }


        driver.Quit();

        return list;
    }

    public async Task<IReadOnlyCollection<CommentItem>> GetComments(string url)
    {
        using var driver = new ChromeDriver(new ChromeOptions());
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(10);
        try
        {
            await driver.Navigate().GoToUrlAsync($"{url}otzyvy/");
        }
        catch (WebDriverTimeoutException e)
        {

        }

        await Task.Delay(TimeSpan.FromSeconds(3));
        var elements = driver.FindElements(By.CssSelector("div[data-meta-name='Collapse__content'] div div span[color='None']"));

        var list = new List<CommentItem>();
        foreach (IWebElement? element in elements)
        {
            var item = new CommentItem();

            item.Text = element.Text;

            list.Add(item);

        }
        driver.Quit();

        return list;
    }

    public async Task<string> GetCommentsText(string url)
    {
        var comments = await GetComments(url);

        var sb = new StringBuilder();

        sb.AppendJoin(Environment.NewLine, comments.Select(c => c.Text));
        return sb.ToString();
        /*return @"
-Дизайн -Бренд (если это кого-то волнует) -Материалы изготовления Кто-то жаловался, что амбушюры маленькие. Для меня это не так, все в порядке. Абсолютно полностью не закрывают уши, но в них довольно комфортно все равно. Радует металлическая вставка в (не знаю, как это называется, штука, соединяющая чаши).
Провод не обрадовал. На вид очень хлипкий, не рекомендую проводить с ним тяжелые эксперименты вроде транспортировки. Звук зреднего качества, но для игр самое то. Отсутствует регулировка громкости на проводе.
Перед покупкой прочитал множество отзывов, которые оставили крайне неприятное впечатление. Так вот, всем остальным, подвергшимся подобному влиянию отзывов хочу сказать, что все далеко не так плохо. Наушники очень приятные, фанатам серии bloody - самое то. Советую.
-Добротное звучание в ценовом сегменте до 3-4 тысяч рублей. -Уши практически не устают в течении 3-5 часов активного использование. Частые аудио/видео-конференции или длинные игровые сессии когда наушники не снимались вовсе - уши не испытывали дискомфорта. -Кабель достаточно длинный. В условиях когда стационарный компьютер находит на полу и кабель вставлен в задний вход на материнской плате - длинны хватает с запасом. -Кабель 3,5 джек + переходник для разъемов микрофон и динамиков. -В играх жанра ""шутеры"" довольно отчетливо можно распознать шаги персонажей, а так же сторону с которой доносятся они. -Микрофон довольно чистый. Особенное если чуть-чуть покопаться в стандартных настройках от Windows. -Отлично работает на Windows 10 и MacOs. -Для использования коммуникации в играх, а так же приложения для общения по типу Skype и Discord-а более чем отлично. -Запись звука для озвучание вполне хороша, но не исключает захвата внешних шумов.
-Спусят 1-1.5 года активного использования, а так же частого переноса в сумке - повредился механизм выдвижного микрофона. -В моем случае спустя полгода в правом наушнике что-то отпало и постоянно гремит в корпусе. -Часто микрофон захватывает внешний шум по типу корпусных вентиляторов от компьютера.
Наушники спустя 1,5 года использования до сих пор в отличном состоянии, не считая сломанного механизма выдвижного микрофона. За приобретаемую сумму в размере 2000 рублей - покупка более чем окупилась и до сих пор выдает хорошее звучание в ценовом сегменте до 4000 рублей. 4/5 из-за хлипкости механизма выдвижного микрофона.
+ Цена, безусловно; + Качество звука просто поражает; + Выдвижной микрофон; + Гибкий непутающийся провод; + Один джек с переходником на два в комплекте.
- На микрофоне китайцы сильно сэкономили; - Малые размеры амбушюр - прижимает уши, не охватывает их.
Гарнитура оставила положительное впечатление. Один джек в сочетании с убирающимся в наушник микрофоном позволяет носить гарнитуру на улице и использовать с телефоном. В комплекте есть переходник на два джека, но, к сожалению, по наблюдениям, из-за него и без того довольно слабый незащищённый микрофон начинает ещё и фонить. Звук просто бомба. Диапазон частот вытягивает как нужно для приятного прослушивания музыки и отличной слышимости в играх. Из-за малого размера наушников в них становится некомфортно через пару часов непрерывного использования - уши прижимает, появляется дискомфорт. Но в целом гарнитура шикарная, хотя уже задумываюсь, что стоило брать модель G501, которая подключается через USB и имеет интересный софт для ""побаловаться"" со звуком.
-Приятный бас, на моей последней ??ат.плате есть ощущение глубокого баса -Неплохая детализация за эту сумму -Очень удобно сидит, как минимум на моей средней голове -Недорогая цена
-Спорный микрофон -Внешний дизайн - на улице с таким стремно выходить
Хоть нет поблизости дорогостоящего оборудования для теста АЧХ, но по своему опыту могу сказать следующее: низы есть, бас присутствует, но он не сильно глубокий на самых простых системах(сильно бюджетных мат.платах), середина и высокие вроде нормальные, лично я могу услышать большинство музыкальных инструментов в средней сцене и прочее. В играх, конкретно в шутерах шаги слышны нормально, но к позиционированию нужно будет привыкать, это не уши условно за 10к и выше. Это моё мнение о звуке слушая музыку как просто через вк/проигрыватель на PC, играя в игры, в том числе и в шутеры, так и смотря фильмы в BDRemux качестве или же сейчас треки для теста в 320 через Adobe Audition CC 2019 - скрин ниже. Использовал наушники не меньше 3-4 лет, сначала это было на мат.плате P8H61-M LE R2.0 с ALC887, а сначала лета 2018 года на MSI X370 SLI PLUS с ALC892. Сидя на прошлой мат.плате к звуку претензий не было за свою цену, по ощущением уровень просто нормальных наушников на 4 балла при их цене, но перейдя на новую, более дорогую с чуть-чуть более современной звуковой карте и системой Audio Boost с Chemi-Con конденсаторами, то бас стал намного приятнее, есть ощущение глубокого баса, да и звук почище стал. В плане микрофона, то он никак не реагирует на PC, проблема не только у меня, хотя тут у людей по отзывам он вроде ОК. Что на win 7, что на win 10 он у меня не работает. Playstation 4 же его видит и работает с ним, по качеству он на уровне 4 баллов за его цену. ИТОГ Лично на 1700 р. примерно, за которые я их покупал - звук и остальное прочее сойдет, если вы сильно ограничены в бюджете и вам сразу нужны наушники. Я же посоветую купить с китая Superlux HD668B или Superlux HD-669, отличные наушники, пусть и без микро, они намного лучше за свою цену, урвете по скидке за 2к - будет вашим лучшим приобретением при малом бюджете, совету посмотреть их обзор от R7GE, он наиболее качественный.
Покупал за 1890, и не пропадал) играю в cs:go, dota2 теперь понятно что для меня важное было все и микрофон и качество звука. Басы на 4 качество на 4 микрофон на 5 ) Очень понравились амбушюры они очень мягкие, гораздо мягче чем на V2 или kraken pro) поверьте есть с чем сравнивать)
Единственный минус для меня что они не полноразмерные) х??тя голова и уши у меня большие) Конечно ещё одна не приятная вещь что амбушюры изготовлены из псевдокожи поэтому уши потеют )приходили друзья говорят отличная гарнитура)
Советую всем, кто не хочет тратить 5000-10000 руб. на компьютерную гарнитуру, а хочет получить хороший звук, отличный микрофон и хорошую шумоизоляцию) а кто пишет что типо тихие или плохой бас КУПИТЕ НОРМАЛЬНУЮ МАТ.ПЛАТУ( привет обладателям асрок) у меня asus p67m и звук отличный) или просто вам не повезло с партией) микрофон тоже пишут что сломался через месяц, пользуюсь пол года и все отлично каждую ночь его складываю а вечером опять достаю - не сломался)

";*/
    }
}