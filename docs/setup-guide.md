\# CoffeeNChill Setup Guide



This guide walks you through setting up and running the CoffeeNChill Part 1 project locally.



\---



\## Prerequisites



Before you begin, make sure you have the following installed:



\- \*\*.NET 10 SDK\*\*

&#x20;— \[Download here](https://dotnet.microsoft.com/download/dotnet/10.0)



\- \*\*Azure Functions Core Tools v4\*\*

&#x20;— Install with `npm install -g azure-functions-core-tools@4`



\- \*\*Docker Desktop\*\*

&#x20;— \[Download here](https://www.docker.com/products/docker-desktop/)



\- \*\*Postman\*\* 

— \[Download here](https://www.postman.com/downloads/)



\### Verify Installations



Run these commands to confirm everything is set up:



dotnet --version

func --version

docker --version



\---



\## Step 1: Clone the Repository



git clone https://github.com/Tamaranaraidoo/CoffeeChill-Part1.git

cd CoffeeChill-Part1



\---



\## Step 2: Start Azurite



Azurite is the local Azure Storage emulator. Run it inside Docker:



docker run -d --name azurite -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite



Verify it is running:



docker ps



You should see the azurite container listed with ports 10000-10002.



\---



\## Step 3: Configure local.settings.json



Create a file called `local.settings.json` inside the `CoffeeNChill/` folder:



{

&#x20; "IsEncrypted": false,

&#x20; "Values": {

&#x20;   "AzureWebJobsStorage": "UseDevelopmentStorage=true",

&#x20;   "FUNCTIONS\_WORKER\_RUNTIME": "dotnet-isolated",

&#x20;   "BlobContainerName": "staffdocuments"

&#x20; }

}



\*\*Note:\*\* This file is in `.gitignore` because it may contain sensitive connection strings.



\---



\## Step 4: Run the Functions



\### Option A: Using Visual Studio



1\. Open `CoffeeNChill.slnx` in Visual Studio

2\. Press \*\*F5\*\* to build and run



\### Option B: Using Command Line



cd CoffeeNChill

func start --dotnet-isolated-debug



Once running, your functions will be available at:



\- \*\*Menu API\*\*: http://localhost:7107/api/menu

\- \*\*Documents API\*\*: http://localhost:7107/api/documents



\---



\## Step 5: Test with Postman



1\. Open Postman

2\. Click \*\*Import\*\* → \*\*Upload Files\*\*

3\. Select `docs/CoffeeNChill.postman\_collection.json`

4\. Run the requests in order:

&#x20;  - Create Menu Item

&#x20;  - Get All Items

&#x20;  - Get by Category

&#x20;  - Update Item

&#x20;  - Delete Item

&#x20;  - Upload Document

&#x20;  - List Documents

&#x20;  - Download Document



\---



\## Docker Containerization



\### Build the Function Image



docker build -t <dockerhub\_tamaranaraidoo>/coffeenchill-functions:v1.0 .



\### Run the Function Container



docker run -d --name coffeenchill-functions -p 7071:80 -e AzureWebJobsStorage="UseDevelopmentStorage=true" --network host <dockerhub\_tamaranaraidoo>/coffeenchill-functions:v1.0



\### Push to Docker Hub



docker login

docker push <dockerhub\_tamaranaraidoo>/coffeenchill-functions:v1.0



\---



\## Troubleshooting



| Issue                       | Solution                                     |

|-----------------------------|----------------------------------------------|

| Azurite connection refused  | Run `docker start azurite`                   |

| Port 7107 already in use    | Stop the other process or change the port    |

| Functions won't build       | Ensure .NET 10 SDK is installed              |

| Cannot connect to Docker    | Open Docker Desktop and wait for it to start |

| local.settings.json missing | Create it manually (see Step 3)              |



\---



\## Project Structure



CoffeeChill-Part1/

├── CoffeeNChill/

│   ├── Functions/

│   │   ├── MenuFunctions.cs

│   │   └── DocumentFunctions.cs

│   ├── Models/

│   │   └── MenuItemEntity.cs

│   ├── Program.cs

│   ├── host.json

│   ├── Dockerfile

│   └── CoffeeNChill.csproj

├── docs/

│   ├── api-documentation.md

│   ├── setup-guide.md

│   └── CoffeeNChill.postman\_collection.json

└── README.md



\---



\## Support



For any issues, refer to the main `README.md` or contact the team.

