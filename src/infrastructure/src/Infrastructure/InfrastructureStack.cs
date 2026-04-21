using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace Infrastructure;

public class InfrastructureStack : Stack
{
    internal InfrastructureStack(Construct scope, string id, IStackProps? props = null) : base(scope, id, props)
    {
        var vpc = new Vpc(this, "DotnetToLambdaVpc", new VpcProps
        {
            IpAddresses = IpAddresses.Cidr("10.0.0.0/16"),
            MaxAzs = 2,
            SubnetConfiguration =
            [
                new SubnetConfiguration
                {
                    CidrMask = 24,
                    SubnetType = SubnetType.PUBLIC,
                    Name = "DotnetToLambdaPublicSubnet"
                },
                new SubnetConfiguration
                {
                    CidrMask = 24,
                    SubnetType = SubnetType.PRIVATE_ISOLATED,
                    Name = "DotnetToLambdaPrivateSubnet"
                },
                new SubnetConfiguration
                {
                    CidrMask = 28,
                    SubnetType = SubnetType.PRIVATE_WITH_EGRESS,
                    Name = "DotnetToLambdaPrivate"
                }
            ],
            NatGateways = 2,
            EnableDnsHostnames = true,
            EnableDnsSupport = true,
        });

        const int dbPort = 3306;

        var db = new DatabaseInstance(this, "DB", new DatabaseInstanceProps
        {
            Vpc = vpc,
            VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PRIVATE_ISOLATED },
            Engine = DatabaseInstanceEngine.Mysql(new MySqlInstanceEngineProps
            {
                Version = MysqlEngineVersion.VER_8_4_4
            }),
            InstanceType = Amazon.CDK.AWS.EC2.InstanceType.Of(InstanceClass.BURSTABLE4_GRAVITON, InstanceSize.MICRO),
            Port = dbPort,
            InstanceIdentifier = "DotnetToLambda",
            BackupRetention = Duration.Seconds(0)
        });

        var privateSubnets = vpc.SelectSubnets(new SubnetSelection
        {
            SubnetType = SubnetType.PRIVATE_ISOLATED
        });

        var natEnabledPrivateSubnets = vpc.SelectSubnets(new SubnetSelection
        {
            SubnetType = SubnetType.PRIVATE_WITH_EGRESS
        });

        var dbSecurityGroup = db.Connections.SecurityGroups[0];

        dbSecurityGroup.AddIngressRule(Peer.Ipv4(vpc.VpcCidrBlock), Port.Tcp(dbPort));

        var parameter = new StringParameter(this, "dev-configuration", new StringParameterProps
        {
            ParameterName = "dotnet-to-lambda-dev",
            StringValue = "{\"CustomerApiEndpoint\": \"https://jsonplaceholder.typicode.com/users\"}",
            DataType = ParameterDataType.TEXT,
            Tier = ParameterTier.STANDARD,
            Description = "Dev configuration for dotnet to lambda",
        });

        _ = new CfnOutput(this, "PrivateSubnet1", new CfnOutputProps
        {
            ExportName = "PrivateSubnet1",
            Value = natEnabledPrivateSubnets.SubnetIds[0]
        });

        _ = new CfnOutput(this, "PrivateSubnet2", new CfnOutputProps
        {
            ExportName = "PrivateSubnet2",
            Value = natEnabledPrivateSubnets.SubnetIds[1]
        });

        _ = new CfnOutput(this, "SecurityGroup", new CfnOutputProps
        {
            ExportName = "DbSecurityGroup",
            Value = dbSecurityGroup.SecurityGroupId
        });

        _ = new CfnOutput(this, "SecretArn", new CfnOutputProps
        {
            ExportName = "DbConnectionStringSecret",
            Value = db.Secret!.SecretArn
        });
    }
}
