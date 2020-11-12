# langbox

Sandbox runner for coding challenges in multiple programming languages using Docker. Written in C# & Blazor for educational purposes at my university.

## Overview

This application allows users to create various challenges using pre-defined templates in multiple programming languages. A challenge is consisted of an environment it runs in & a test file, that has to be run against a provided solution.

The solution provided by a user will be run in a sandbox using Docker. This project defines sandbox templates, that have to be build before attempting to run this application.

## Building the application

Before running the application, there are some steps you have to do, in order for the application to run correctly.

### Building sandbox templates

#### Requirements

- Docker

Sandbox templates are Docker images and they are located in the project root under directory `SandboxTemplates`. You will find instructions below on how to setup & build these images.

#### C# (MSUnit) template

Navigate to the `SandboxTemplates/Lgbox-CSharp-MSUnit` directory & run command `docker build -t lgbox/csharp-msunit-template:0.1.0 .` - after this step, this template will be available to the application.

#### Java (JUnit) template

Navigate to the `SandboxTemplates/Lgbox-Java-JUnit` directory & run command `docker build -t lgbox/java-junit-template:0.1.0 .` - after this step, this template will be available to the application.

### Building & running using Visual Studio

#### Requirements

- Docker (with `docker-compose` installed aswell)
- Visual Studio (2019 or greater)
- .NET Framework
- PostgreSQL database

The application was developed using Visual Studio and so it has configured running tasks. Although the application can be run through the `IIS Express` task, there are several things that have to be set-up first:

- You have to build Docker images of sandbox templates, as explained in previous step
- You will have to run a PostgreSQL database. For local development, you can use the `docker-compose.yml` file to spin up only the database, using command `docker-compose up db`
- Using Visual Studio, you will have to specify a `DATABASE_URL` environment variable to the task you wish to run (for example, the `IIS Express` task) - this will have to point to a PostgreSQL database (example: `postgres://postgres@db:5432/langbox`)

Now you can run the application using `IIS Express` run task. This will automatically open a browser with the client application.