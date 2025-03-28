# Data Dashboard

This solution was released as Discovered Insights and ran for a couple of years as a live subscription service. Subscribers were able to access data gathered from a variety of quantitative studies, visualised as charts from which they could gain insight into topics of interest. The data could be custom queried and charts could be exported for use in documents.

Here are some screenshots of the Data Dashboard:

![alt text](https://github.com/clivehoward/DataDashboardDev/blob/master/DataDashboardWeb/wwwroot/images/Screenshot%20-1.png?raw=true)
![alt text](https://github.com/clivehoward/DataDashboardDev/blob/master/DataDashboardWeb/wwwroot/images/Screenshot%20-2.png?raw=true)
![alt text](https://github.com/clivehoward/DataDashboardDev/blob/master/DataDashboardWeb/wwwroot/images/Screenshot%20-3.png?raw=true)

The web app also has a secure admin area for managing subscriptions and handling purchase options such as discount codes.

## Technology

The solution contains several projects created using .NET Framework 4.7 and .NET Core 2.0. The solution was deployed to Microsoft Azure using two Azure App Services and an Azure Function connected to a Queue.

## Applications

#### Data Dashboard API
This project is an ASP.NET Framework 4.7 Web API that provides data to the Data Dashboard Web App. It connects to a SQL Server database and provides data to the Web App via RESTful endpoints.

#### Data Dashboard Web
This project is an ASP.NET .NET Core 2.0 Web App that provides a user interface for the Data Dashboard. It connects to the Data Dashboard API to retrieve data.

## Serverless

#### Data Dashboard Function
This project is an Azure Function that processes messages from a Queue.

## Libraries

These libraries are shared between the Data Dashboard API and Data Dashboard Web projects. DataDashboardWebLib is used by Data Dashboard Web to interact with the Data Dashboard API.

## Services

The following services are used by the Data Dashboard solution:

#### Redis
This is used for caching data.

#### Azure SQL Database
This is used to store data.

#### Azure Blob Storage
This is used to store files.

#### Azure Queue Storage
This is used to store email messages for the Data Dashboard Function.

#### Stripe
This is used to process subscriber payments

#### SendGrid
This is used to send emails.

#### Application Insights
This is used to monitor the solution.

## Deployment

The solution was deployed to Azure using Azure DevOps. The deployment process was automated using Azure DevOps pipelines.

## Notes

This solution was designed and created by myself and is here simply as an example of previous work. It is no longer being used commercially and there is no data stored in this solution.