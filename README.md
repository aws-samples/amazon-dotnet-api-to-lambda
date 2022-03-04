# Migrating a .NET Core Web Api to Serverless Compute

This repo contains sample code that is related to the AWS blog post here **link to be added** on migrating a .NET core Web Api to use Serverless compute. The API is a simple one that implements functionality for storing/retriving airline bookings. It interacts with:

- A MySQL database
- An external customer service API accessed over HTTP

## Local .NET Core API Execution

The starting .NET Core Web API can be executed on your local machine using Docker. Follow the below steps to start the application:

1. Start the MySQL instance locally using docker-compose

``` bash
docker-compose up -d
```

2. Navigate into the DotnetToLambda.Api folder and start the API

``` bash
cd .\src\DotnetToLambda.Api\DotnetToLambda.Api\
dotnet run
```

3. Make a POST request to http://localhost:5000/booking/reserve with the below body:

```json
{
    "OutboundFlightId": "1234567",
    "CustomerId": "customername",
    "ChargeId": "98765"
}
```

4. Make a GET request as per below to validate the API is online, you should receive back on record.
``` bash
curl http://localhost:5000/booking/customer/customername
```

## Serverless Deployment

The AWS ready application is built in two parts that need to be deployed in order.

### 1. Infrastructure

All required infrastructure (VPC, Subnets, RDS instances) are deployed using AWS CDK. To deploy:

1. Navigate into the infrastructure directory and deploy the CDK application
``` bash
cd .\src\DotnetToLambda.Api\infrastructure\
cdk deploy
```

2. Make a note of all the CDK outputs as these will be required to deploy the Serverless application 

### 2. Booking API

Secondly, the actual Booking API running on Lambda compute is deployed using the SAM Cli.

1. Navigate into the serverless application folder and build the sam application
``` bash
cd .\src\serverless\DotnetToLambda.Serverless\
sam build
```
2. Once the build is complete run the deploy command below, replacing the parameter values with the outputs taken from the CDK deployment
```bash

sam deploy --parameter-overrides 'ParameterKey=SecretArn,ParameterValue=<SecretArn> ParameterKey=PrivateSubnet1,ParameterValue=<PrivateSubnet1> ParameterKey=PrivateSubnet2,ParameterValue=<PrivateSubnet2> ParameterKey=SecurityGroup,ParameterValue=<SecurityGroup>'

```

## Security

See [CONTRIBUTING](CONTRIBUTING.md#security-issue-notifications) for more information.

## License

This library is licensed under the MIT-0 License. See the LICENSE file.