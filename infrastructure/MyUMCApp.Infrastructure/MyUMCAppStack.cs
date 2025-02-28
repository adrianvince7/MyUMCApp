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
using System.Collections.Generic;
using System.Runtime.InteropServices.JavaScript;
using Amazon.CDK.AWS.SNS.Subscriptions;
using Function = Amazon.CDK.AWS.Lambda.Function;

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
            InstanceType = Amazon.CDK.AWS.EC2.InstanceType.T2_MICRO, // Free tier eligible
            Vpc = null,
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
        Function? imageOptimizer;
        imageOptimizer = new Function(this, "ImageOptimizer", new Amazon.CDK.AWS.Lambda.FunctionProps        {
            Runtime = Runtime.NODEJS_18_X,
            Handler = "index.handler",
            Code = Code.FromAsset("lambda/image-optimizer"),
            MemorySize = 1024,
            Timeout = Duration.Seconds(5),
            Environment = new Dictionary<string, string>
            {
                { "NODE_ENV", "production" }
            }
        });
        var distribution = new Distribution(this, "MyUMCAppDistribution", new DistributionProps
        {
            DefaultBehavior = new BehaviorOptions
            {
                Origin = new S3Origin(websiteBucket),
                ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                AllowedMethods = AllowedMethods.ALLOW_GET_HEAD,
                CachedMethods = CachedMethods.CACHE_GET_HEAD,
                CachePolicy = new CachePolicy(this, "DefaultCachePolicy", new CachePolicyProps
                {
                    DefaultTtl = Duration.Days(1),
                    MinTtl = Duration.Minutes(1),
                    MaxTtl = Duration.Days(365),
                    EnableAcceptEncodingGzip = true,
                    EnableAcceptEncodingBrotli = true,
                    QueryStringBehavior = CacheQueryStringBehavior.None()
                })
            },
            AdditionalBehaviors = (IDictionary<string, IBehaviorOptions>) new Dictionary<string, BehaviorOptions>
            {
                {
                    "profiles/*", new BehaviorOptions
                    {
                        Origin = new S3Origin(websiteBucket),
                        ViewerProtocolPolicy = ViewerProtocolPolicy.REDIRECT_TO_HTTPS,
                        AllowedMethods = AllowedMethods.ALLOW_GET_HEAD,
                        CachedMethods = CachedMethods.CACHE_GET_HEAD,
                        CachePolicy = new CachePolicy(this, "ProfileImagesCachePolicy", new CachePolicyProps
                        {
                            DefaultTtl = Duration.Days(30),
                            MinTtl = Duration.Hours(1),
                            MaxTtl = Duration.Days(365),
                            EnableAcceptEncodingGzip = true,
                            EnableAcceptEncodingBrotli = true,
                            HeaderBehavior = CacheHeaderBehavior.AllowList(new[]
                                { "Origin", "Access-Control-Request-Method", "Access-Control-Request-Headers" }),
                            QueryStringBehavior = CacheQueryStringBehavior.AllowList(new[] { "w", "h", "q" }),
                            CookieBehavior = CacheCookieBehavior.None()
                        }),
                        EdgeLambdas = new[]
                        {
                            new EdgeLambda
                            {
                                EventType = LambdaEdgeEventType.ORIGIN_RESPONSE,
                                FunctionVersion = imageOptimizer.CurrentVersion
                            }
                        }
                    }
                }
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

        // Lambda@Edge for image optimization

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
    }
}