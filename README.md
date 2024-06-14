
# Durable Function Dollar Observer Example

## Description

This repository provides an example of using Azure Durable Functions to create a Dollar Observer application. It demonstrates how to set up and use Azure Durable Functions to orchestrate workflows and manage state in a serverless environment, which is useful for developers looking to build reliable, scalable applications with complex workflows.

## Requirements

- Node.js
- Azure Account with Azure Functions setup
- Azure Functions Core Tools
- Azure CLI
- Yarn or npm for package management

## Mode of Use

1. Clone the repository:
   ```bash
   git clone https://github.com/ferrerallan/durable-function-dollarobserver.git
   ```
2. Navigate to the project directory:
   ```bash
   cd durable-function-dollarobserver
   ```
3. Install the dependencies:
   ```bash
   yarn install
   ```
4. Ensure you have an Azure account and have set up Azure Functions.
5. Install Azure Functions Core Tools and Azure CLI.
6. Deploy the Azure Function:
   ```bash
   func azure functionapp publish <FunctionAppName>
   ```

## Implementation Details

- **functions/**: Contains the Azure Functions code for the Dollar Observer application.
- **package.json**: Configuration file for the Node.js project, including dependencies.
- **local.settings.json**: Configuration file for local development settings.

### Example of Use

Here is an example of how to implement an orchestrator function in Azure Durable Functions:

```javascript
const df = require("durable-functions");

module.exports = df.orchestrator(function* (context) {
  const input = context.df.getInput();
  const output = [];

  output.push(yield context.df.callActivity("ActivityFunctionName", input));
  output.push(yield context.df.callActivity("AnotherActivityFunctionName", input));

  return output;
});
```

This code defines an orchestrator function that coordinates the execution of multiple activity functions, passing input and collecting output from each activity.

## License

This project is licensed under the MIT License.

You can access the repository [here](https://github.com/ferrerallan/durable-function-dollarobserver).
