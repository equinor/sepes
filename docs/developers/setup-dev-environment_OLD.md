# Dev environment setup

## Prerequisites:

To be able to run and develop for this project there are a some runtimes that need to be installed.

* [Dotnet Core SDK 3.0](https://dotnet.microsoft.com/download)

* [Node.js](https://nodejs.org/en/)

* [Microsoft SQL Server](https://www.microsoft.com/nb-no/sql-server/sql-server-downloads)

### Azure
These services are required
* AD app
* Subscribtions
* Application Insight



## Add dependencies

In both the Sepes.RestApi and Sepes.RestApi.Test folder run the command 
```
dotnet restore
```

In the FrontEnd folder you need to run the command 
```
npm install
```


## Setup config:

All values bellow are written in without quotation marks
```
SEPES_NAME=
```
This is the name that will be used to create resources within azure. Do not include spaces.
```
SEPES_TENANT_ID=            
```
```
SEPES_CLIENT_ID=            
```
```
SEPES_CLIENT_SECRET=        
```
```
SEPES_INSTRUMENTATION_KEY=  
```
This is found in the Overview tab foun in the Application Insights service created in Azure.
```
SEPES_SUBSCRIPTION_ID=      
```
This is the Subscrition ID of the subscribtion sepes will use for its operation
```
SEPES_MSSQL_CONNECTION_STRING=
```
Needs to be in following format: 
```
Data Source={ip or url to server};Initial Catalog={name of catalog};User ID={userID};Password={password}
```
```
SEPES_HTTP_ONLY=false
```
This should only be set to true if you are intending to run SEPES behind some other proxy that will provide encryption, like for example Docker.

    
## Setup database
* Option 1: Use SQL Query
    * Create or have an SQL Server
    * Open a connection to SQL Server
    * Use the query file or copy its contents into the management softwares query editor.

* Option 2:
    * Use the full server copy and import it into Microsoft SQL Server Management Studio.
    * You need an existing sql server on Azure you can target for deployment
    * Use Microsoft SQL Server Management Studio to locally import.
    * Right click database and select Tasks>Deploy to azure

## Setup monitoring service.
* Create an Application Insights instance for SEPES
* In the overview tab copy the "Instrumentation Key" and paste into the .env file as described above.
* Boot up an instance of SEPES to verify that it logs correctly to Application Insights
## Common issues:

Error: 
```
"Failed to load resource: net::ERR_CERT_AUTHORITY_INVALID"
```
Solution:
Run the below command
```
dotnet dev-certs https --clean
```
then after that command succesfully executes, run
```
dotnet dev-certs https --trust
```
This should reinstal the dev-certificate