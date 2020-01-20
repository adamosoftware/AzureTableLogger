I know there are quite a few logging tools and apps out there -- from open source projects like [Serilog](https://serilog.net/) and [NLog](https://nlog-project.org/) to mature and pricey APM tools like [Rollbar](https://rollbar.com/), [DataDog](https://www.datadoghq.com/) and [Raygun](https://raygun.com/). Naturally, therefore, it's time to make my own!!! Haha just kidding -- sort of.

My problem with existing tools like Serilog and NLog is that they just aren't simple enough. They have a bazillion settings and options. Maybe I am overreacting or allowing myself to be intimidated, but I'm looking for a logging solution with bare-minimum impact on my code,  uses only Azure Table Storage, and lets me implement a robust and simple unhandled exception page in web apps. I simply don't care about the myriad of logging destinations I *could* use. Azure Table Storage is perfectly fine for this purpose, IMO. My problem with the paid tools is that I don't really want to spend any money on this functionality apart from my Azure subscription. I think the expensive tools make a lot of big promises about "root cause analysis" that at this point I'm sort of jaded about. All I want is a simple, low-impact logging solution, and to offer it to anyone out there who might be interested in such a thing also.

If I wanted to attach notifications and analytics to this log data, I could approach that as a downstream or phase 2 project that wouldn't depend on the core logging service.