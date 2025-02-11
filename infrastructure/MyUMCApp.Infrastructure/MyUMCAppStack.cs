using Amazon.CDK;
using Amazon.CDK.AWS.Cognito;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.S3;
using Amazon.CDK.AWS.CloudFront;
using Amazon.CDK.AWS.CloudFront.Origins;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SNS;
using Amazon.CDK.AWS.SQS;
using Constructs;

namespace MyUMCApp.Infrastructure;

public class MyUMCAppStack : Stack
{
    public MyUMCAppStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
    {
        // VPC
        var vpc = new Vpc(this, "MyUMCAppVPC", new VpcProps
        {
            MaxAzs = 2,
            NatGateways = 0 // Using AWS Free Tier
        });

        // Cognito User Pool
        var userPool = new UserPool(this, "MyUMCAppUserPool", new UserPoolProps
        {
            UserPoolName = "myumcapp-users",
            SelfSignUpEnabled = true,
            SignInAliases = new SignInAliases
            {
                Email = true
            },
            StandardAttributes = new StandardAttributes
            {
                Email = new StandardAttribute { Required = true, Mutable = true }
            }
        });

        var userPoolClient = new UserPoolClient(this, "MyUMCAppUserPoolClient", new UserPoolClientProps
        {
            UserPool = userPool,
            GenerateSecret = false,
            OAuth = new OAuthSettings
            {
                Flows = new OAuthFlows
                {
                    AuthorizationCodeGrant = true
                },
                Scopes = new[] { OAuthScope.EMAIL, OAuthScope.OPENID, OAuthScope.PROFILE }
            }
        });

        // RDS MySQL Instance
        var database = new DatabaseInstance(this, "MyUMCAppDatabase", new DatabaseInstanceProps
        {
            Engine = DatabaseInstanceEngine.MYSQL,
            InstanceType = InstanceType.T2_MICRO, // Free tier eligible
            Vpc = vpc,
            AllocatedStorage = 20,
            MaxAllocatedStorage = 20,
            PubliclyAccessible = false
        });

        // S3 Bucket for Static Website
        var websiteBucket = new Bucket(this, "MyUMCAppWebsite", new BucketProps
        {
            WebsiteIndexDocument = "index.html",
            PublicReadAccess = false,
            RemovalPolicy = RemovalPolicy.DESTROY,
            AutoDeleteObjects = true
        });

        // CloudFront Distribution
        var distribution = new Distribution(this, "MyUMCAppDistribution", new DistributionProps
        {
            DefaultBehavior = new BehaviorOptions
            {
                Origin = new S3Origin(websiteBucket),
                ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                AllowedMethods = AllowedMethods.ALLOW_GET_HEAD,
                CachedMethods = CachedMethods.CACHE_GET_HEAD
            },
            DefaultRootObject = "index.html",
            ErrorResponses = new[]
            {
                new ErrorResponse
                {
                    HttpStatus = 404,
                    ResponseHttpStatus = 200,
                    ResponsePagePath = "/index.html"
                }
            }
        });

        // SNS Topics
        var eventsTopic = new Topic(this, "MyUMCAppEventsTopic", new TopicProps
        {
            TopicName = "myumcapp-events"
        });

        var announcementsTopic = new Topic(this, "MyUMCAppAnnouncementsTopic", new TopicProps
        {
            TopicName = "myumcapp-announcements"
        });

        // SQS Queues
        var eventsQueue = new Queue(this, "MyUMCAppEventsQueue", new QueueProps
        {
            QueueName = "myumcapp-events-queue"
        });

        var announcementsQueue = new Queue(this, "MyUMCAppAnnouncementsQueue", new QueueProps
        {
            QueueName = "myumcapp-announcements-queue"
        });

        // Subscribe queues to topics
        eventsTopic.AddSubscription(new SqsSubscription(eventsQueue));
        announcementsTopic.AddSubscription(new SqsSubscription(announcementsQueue));

        // Output values
        new CfnOutput(this, "UserPoolId", new CfnOutputProps { Value = userPool.UserPoolId });
        new CfnOutput(this, "UserPoolClientId", new CfnOutputProps { Value = userPoolClient.UserPoolClientId });
        new CfnOutput(this, "WebsiteBucketName", new CfnOutputProps { Value = websiteBucket.BucketName });
        new CfnOutput(this, "CloudFrontURL", new CfnOutputProps { Value = distribution.DistributionDomainName });
 