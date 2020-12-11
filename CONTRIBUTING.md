# Contributing to WITSML Explorer
Contributions are welcome and greatly appreciated.

## Set up development environment
After forking the repo to your own github account, do the following:
```
# Clone the repo
git clone git@github.com:equinor/witsml-explorer.git

# Step into local repo
cd witsml-explorer

# Create your own local mysettings.json file (not to be tracked in git)
cd Src/WitsmlExplorer.Api/
cp appsettings.json mysettings.json
```

### Using MongoDB
_Note: WE also supports Cosmos DB, but that requires a little more setup, see the [CosmosDB setup guide](#Cosmos-database)._

To quickly get a development database up and running, a MongoDB docker image can be used. 
```
# From the project root, cd to
cd Docker/MongoDb
```
Add an initial db username and password by editing the `docker-compose.yml` file and setting `MONGO_INITDB_ROOT_USERNAME` and `MONGO_INITDB_ROOT_PASSWORD` (no space after `=`).
```
# Pull and run a default MongoDB locally
docker-compose up -d
```
The default is to mount a volume in the same directory, but that can be changed in the `docker-compose.yml` file based on your preference. 

Add the following configuration to `mysettings.json` so that the backend will be able to connect to our new database:
```
"MongoDb": {
  "Name": "witsml-explorer-db",
  "ConnectionString": "mongodb://username:password@localhost"
},
```
`username` and `password` is what was configured in the docker-compose.yml file.

#### Populate list of WITSML servers
The list of WITSML servers a user can connect to is currently not editable by the user itself (feature is disabled until proper authorization is implemented).
There exists some integration tests that can be run for this purpose.
The file containing the relevant tests can be found at `Tests/WitsmlExplorer.IntegrationTests/Api/Repositories/MongoDbRepositoryTests.cs`
Remember to add the same MongoDb configuration as described in the previous section to the configuration file for the [integration tests project](#Integration-tests).

## Running
The database, backend and frontend must be running at the same time for WE to work properly.

### Backend
```
cd Src/WitsmlExplorer.Api/
# Download dependencies and build project
dotnet build
# Run the backend
dotnet run
```
In folder `Src/WitsmlExplorer.Api/` run `dotnet build`  and `dotnet run`

### Frontend
```
cd Src/WitsmlExplorer.Frontend/
# Download dependencies
yarn
# Run the frontend
yarn dev
```
You should now find WitsmlExplorer running on `localhost:3000` in your browser. Ensure that frontend, backend and database are running. 

## Testing

### Frontend
```
# From project root
cd Src/WitsmlExplorer.Frontend
yarn test
```

### Backend

#### Unit tests
```
# From the project root
cd Tests/WitsmlExplorer.Api.Tests
dotnet test
```

#### Integration tests
The purpose of these tests has been to test workers and integrations directly against WITSML servers. They are by default skipped, and not part of the test suite that is run during the CI pipeline.  

You will need a secrets file for keeping the credentials for the server you wish to run the tests against:
```
# From the project root
cd Tests/WitsmlExplorer.IntegrationTests
# Create a JSON file for WITSML server secrets
touch secrets.json
```
The file should contain these fields if running tests against a given WITSML server:
```json
{
  "Witsml": {
    "Host": "<witsml server url>",
    "Username": "<username>",
    "Password": "<password>"
  }
}
```
A db configuration is needed if running tests that uses the database:
```json
{
  "MongoDb": {
    "Name": "witsml-explorer-db",
    "ConnectionString": "mongodb://username:password@localhost"
  }
}
```

To run a given test, open the test file that contains it and remove the `Skip` part. E.g replace
``` c#
[Fact(Skip = "Should only be run manually")]
```
with
``` c#
[Fact]
```

Then run
```
dotnet test
```

## Code style guidelines
We use some tools to help us keep the code style as consistent as possible. Automated checks are ran when a PR is opened. The build will break if these rules are not enforced.

### Prettier [![code style: prettier](https://img.shields.io/badge/code_style-prettier-ff69b4.svg?style=flat-square)](https://github.com/prettier/prettier)
In our frontend project we use the opinionated code formatter [Prettier](https://prettier.io/). Most of the rules applied are the default, but some changes can be found in `.prettierrc`.
Most IDEs have plugins that support Prettier. This will make the result of formatting code in your IDE be consistent with running prettier manually. 

### ESLint
For linting our frontend code we use [ESLint](https://github.com/typescript-eslint/typescript-eslint).

### ECLint
For our non frontend code, we use [ECLint](https://github.com/jedmao/eclint) for validating and fixing code that does not follow the project rules. They can be found in `.editorconfig` at the project root.

### Run checks as a pre-commit hook
We use [Husky](https://github.com/typicode/husky) to run `ESLint` and `ECLint` as pre commit hooks. This will give errors when creating commits that causes checks to fail.

## Project overview
Here you will get a brief overview of the system flow and project structure.

### Project structure summary
This solution consists of 3 projects:
* Witsml
  * Contains domain objects which maps to the XML structure from a Witsml server. It also contains functionality to build queries for a set of Witsml objects (well, wellbore, rig, log).
* WitsmlExplorer.Api
  * Api used by the frontend application. Every request from the frontend will be handled here. Receive job descriptions and spawn workers for every writing operations done on a witsml server.
* WitsmlExplorer.Frontend
  * Frontend web application which is what the user sees and interacts with.

### Simplified flow
This diagram gives a quick overview over the application components and flows.

<img src="./flow-chart.svg">

* When the user navigates in the web application, WITSML data is retrieved.
* When the user adds/updates/deletes, a job is made and a worker is triggered asynchronously on the backend.
* After a worker has finished, the result message will be pushed to the client, and a refresh is triggered if necessary.

### WITSML server credentials flow
Every request run against a WITSML server is run from the backend and needs to be authenticated.
Basic auth is required for a lot of servers, so that is currently the only way WE authenticate against them.

Most actions done by the user in WE involves fetching or writing data to external WITSML servers. All these requests require credentials to be provided. 
It would be a very bad user experience if the user would have to provide credentials for every request. 
Therefore an encrypted version of the passwords is saved both on the frontend and backend.
The webapp only stores them for a limited amount of time, and will provide them for every request involving WITSML servers. 
The backend has a [Data Protection](https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/introduction) storage running in memory, which is used for creating the encrypted passwords as well as decrypting them (only possible for that running instance).
Whenever a request is run towards a WITSML server, the backend will decrypt the password, and use it when running the request against the given WITSML server.

This is how the flow is when a user has selected a server and will need to authenticate against it. After this is done, a fresh list of wells is fetched.  
<img src="./credentials-flow.svg">

## Additional information 
### Cosmos database
Witsml Explorer requires a database to store application data. One option is to use a Cosmos database in Azure.

#### Setting up a database in Azure 
1) If not already installed, install Azure CLI on your computer [Azure CLI](https://docs.microsoft.com/en-us/cli/azure/?view=azure-cli-latest), and then login `az login`
2) Copy `config-example.cfg` into a new file `config.cfg` in folder `Scripts/Azure` and fill in `subscriptionId` and `resourceGroupName` from your Azure subscription and resourcegroup.
3) There exist some scripts  that may simplify setting up the necessary infrastructure for this project in folder `Scripts/Azure`. 
<br>Script to create Cosmos DB: ```./create-cosmos-db.sh```
<br>Script to run all together: ```./run-azure-scripts.sh```
4) In file `config.cfg` enter `databaseAccountName` and a name (container) for your database in `databaseName`.  
5) Run ```./create-cosmos-db.sh``` (prerequsite azure cli installed, and that your are logged in)

#### Configure backend to use CosmosDB
If you have a CosmosDB setup and ready, follow these steps to configure the backend properly.
```
#From project root.
cd Src/WitsmlExplorer.Api
# If you do not have a mysettings.json file, create it:
cp appsettings.json mysettings.json 
``` 
Add the following `"CosmosDb"` configuration to `mysettings.json`
```
{
  {...},
  "CosmosDb": {
    "Uri": "<...>", (Uri from relevant Azure Database => Overview => Uri )
    "Name": "<...>", (Container name from relevant Azure Database => DataExplorer || databaseName from config.cfg)
    "AuthKey": "<...>" (PrimaryKey from relevant Azure Database => Setting => Keys )
  },
  {...}
}
```

### Generating service references
_Note that this only documents how it was done. It is not necessary to repeat this until incorporating changes to wsdl_.
Install `dotnet-svcutil`: `dotnet tool install --global dotnet-svcutil`

`ServiceReference` is generated by executing (in project folder): `dotnet-svcutil ../../Resources/Wsdl/witsml_v1.4.0_api.wsdl --namespace "*,Witsml.ServiceReference" --outputFile WitsmlService`