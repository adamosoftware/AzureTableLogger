I know there are quite a few logging tools and apps out there -- from open source projects with large followings like [Serilog](https://serilog.net/) and [NLog](https://nlog-project.org/) to mature and pricey enterprise APM tools like [Rollbar](https://rollbar.com/), [DataDog](https://www.datadoghq.com/) and [Raygun](https://raygun.com/). Naturally, therefore, it's time to make my own!!! Haha just kidding -- sort of.

My problem with existing tools like Serilog and NLog is that they just aren't simple enough. They have a bazillion settings and options. Maybe I am overreacting or allowing myself to be intimidated, but I'm looking for a logging solution with bare-minimum impact on my code,  uses only Azure Table Storage (or Cosmos), and lets me implement a robust and simple unhandled exception page in web apps. Something that *does not make me think.* I simply don't care about the myriad of logging destinations I *could* use. Azure Table Storage is perfectly fine for this purpose, IMO. My problem with the paid tools is that I don't really want to spend any money on this functionality apart from my Azure subscription. I think the expensive tools make a lot of big promises about "root cause analysis" that at this point I'm sort of jaded about. All I want is a simple, low-impact logging solution, and to offer it to anyone out there who might be interested in such a thing also.

If I wanted to attach notifications and analytics to this log data, I could approach that as a downstream or phase 2 project that wouldn't depend on the core logging service.

There are two Nuget packages in this repo:

- **AO.AzureTableLogger** provides Exception logging at the lowest level. This package has no dependencies apart from Azure Storage itself. Have a look at the tests to see this in action: [simplest case](https://github.com/adamosoftware/AzureTableLogger/blob/master/Testing/LoggingTests.cs#L26), with [custom data](https://github.com/adamosoftware/AzureTableLogger/blob/master/Testing/LoggingTests.cs#L61), [log and retrieve](https://github.com/adamosoftware/AzureTableLogger/blob/master/Testing/LoggingTests.cs#L78), and [nested messages](https://github.com/adamosoftware/AzureTableLogger/blob/master/Testing/LoggingTests.cs#L95)

- **AO.AzureTableLogger.AspNetCore** is intended for .NET Core 3 apps, and is specifically meant for creating your own global exception filter. Please see my [walkthrough](https://github.com/adamosoftware/AzureTableLogger/wiki/SampleApp-walkthrough) on how to implement.

Note that I omitted my storage credentials from the repo. If you clone and run this yourself, create a file called `config.json` both in the SampleApp and Testing projects that looks like this:

```json
{
    "StorageAccount" : {
        "Name" : "<your account>",
        "Key" : "<your key>"
    }
}
```

## Exception Dashboard Razor Class Library

I also offer a Razor Class Library Nuget package **AO.AzureTableLogger.ExceptionDashboard**. Use this to view exceptions logged by AO.AzureTableLogger, both as a flat, searchable list, as well as grouped by frequency as "top issues." It's free to use offline, and fully functional. To use online (somewhere besides "localhost"), please [purchase a license key](https://paypal.me/adamosoftware?locale.x=en_US) for $150 USD. This is a great way to get visiblity into unhandled exceptions in your app without recurring cost,  database traffic, nor any code assets to own.

To implement in your app:

1. Install Nuget package **AO.AzureTableLogger.ExceptionDashboard**
2. Implement **AO.AzureTableLogger.AspNetCore** using this [walkthrough](https://github.com/adamosoftware/AzureTableLogger/wiki/SampleApp-walkthrough).
3. Run your application, and navigate to `/ExceptionLog/TopIssues` or `/ExceptionLog/List`. When running on localhost, you'll see the Exception Dashboard pages.
4. To enable the dashboard to work online, purchase a license key, then add it to your app's startup services, following [this guide](https://github.com/adamosoftware/AzureTableLogger/wiki/Register-Exception-Dashboard).

![img](dashboard.gif)
