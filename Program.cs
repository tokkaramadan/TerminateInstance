using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;

using Amazon;
using Amazon.EC2;
using Amazon.EC2.Model;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;
using Amazon.S3;
using Amazon.S3.Model;

namespace terminate_instance
{
    class Program
    {
        public static void Main(string[] args)
        {
            Console.Write(GetServiceOutput());
            Console.Read();
            TerminateInstance();
        }

      
        public static void TerminateInstance()
        {
            List<string> instanceId = new List<string>();
            string input = Console.ReadLine();

            if (input == "")
            {
                Console.WriteLine("Please enter the instance ID then hit 'enter' ");
                 input = Console.ReadLine();
                instanceId.Add(input);
            }
                
            while (input != "exit")
            {
                Console.WriteLine("Please enter another instance ID then hit 'enter' to enter another ID , if you finished the IDs and want to proceed please type ' exit '");
                input = Console.ReadLine();

                  if (input == "exit")
                {
                    foreach (string value in instanceId)
                    {
                        Console.WriteLine("The IDs that was entered are : " + " " + value);
                    }
                    Console.WriteLine("please hit enter ");
                    Console.ReadLine();
                }
                else
                    instanceId.Add(input);
            }


                var ec2Client = new AmazonEC2Client();
            var request = new TerminateInstancesRequest();
            request.InstanceIds = new List<string>();
            request.InstanceIds = instanceId;

            try
            {
                var response = ec2Client.TerminateInstances(request);
                foreach (InstanceStateChange item in response.TerminatingInstances)
                {
                    Console.WriteLine("Terminated instance: " + item.InstanceId);
                    Console.WriteLine("Instance state: " + item.CurrentState.Name);
                }
            }
            catch (AmazonEC2Exception ex)
            {
                // Check the ErrorCode to see if the instance does not exist.
                if ("InvalidInstanceID.NotFound" == ex.ErrorCode)
                {
                    Console.WriteLine("Instance {0} does not exist.", instanceId);
                }
                else
                {
                    // The exception was thrown for another reason, so re-throw the exception.
                    throw;
                }
            }
        }

        public static string GetServiceOutput()
        {
            StringBuilder sb = new StringBuilder(1024);
            using (StringWriter sr = new StringWriter(sb))
            {
                sr.WriteLine("===========================================");
                sr.WriteLine("Welcome to the AWS .NET SDK!");
                sr.WriteLine("===========================================");

                // Print the number of Amazon EC2 instances.
                IAmazonEC2 ec2 = new AmazonEC2Client();
                DescribeInstancesRequest ec2Request = new DescribeInstancesRequest();

                try
                {
                    DescribeInstancesResponse ec2Response = ec2.DescribeInstances(ec2Request);
                    int numInstances = 0;
                    numInstances = ec2Response.Reservations.Count;
                    sr.WriteLine(string.Format("You have {0} Amazon EC2 instance(s) running in the {1} region.",
                                               numInstances, ConfigurationManager.AppSettings["AWSRegion"]));
                }
                catch (AmazonEC2Exception ex)
                {
                    if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
                    {
                        sr.WriteLine("The account you are using is not signed up for Amazon EC2.");
                        sr.WriteLine("You can sign up for Amazon EC2 at http://aws.amazon.com/ec2");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Error Type: " + ex.ErrorType);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine();

                // Print the number of Amazon SimpleDB domains.
                IAmazonSimpleDB sdb = new AmazonSimpleDBClient();
                ListDomainsRequest sdbRequest = new ListDomainsRequest();

                try
                {
                    ListDomainsResponse sdbResponse = sdb.ListDomains(sdbRequest);

                    int numDomains = 0;
                    numDomains = sdbResponse.DomainNames.Count;
                    sr.WriteLine(string.Format("You have {0} Amazon SimpleDB domain(s) in the {1} region.",
                                               numDomains, ConfigurationManager.AppSettings["AWSRegion"]));
                }
                catch (AmazonSimpleDBException ex)
                {
                    if (ex.ErrorCode != null && ex.ErrorCode.Equals("AuthFailure"))
                    {
                        sr.WriteLine("The account you are using is not signed up for Amazon SimpleDB.");
                        sr.WriteLine("You can sign up for Amazon SimpleDB at http://aws.amazon.com/simpledb");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Error Type: " + ex.ErrorType);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine();

                // Print the number of Amazon S3 Buckets.
                IAmazonS3 s3Client = new AmazonS3Client();

                try
                {
                    ListBucketsResponse response = s3Client.ListBuckets();
                    int numBuckets = 0;
                    if (response.Buckets != null &&
                        response.Buckets.Count > 0)
                    {
                        numBuckets = response.Buckets.Count;
                    }
                    sr.WriteLine("You have " + numBuckets + " Amazon S3 bucket(s).");
                }
                catch (AmazonS3Exception ex)
                {
                    if (ex.ErrorCode != null && (ex.ErrorCode.Equals("InvalidAccessKeyId") ||
                        ex.ErrorCode.Equals("InvalidSecurity")))
                    {
                        sr.WriteLine("Please check the provided AWS Credentials.");
                        sr.WriteLine("If you haven't signed up for Amazon S3, please visit http://aws.amazon.com/s3");
                    }
                    else
                    {
                        sr.WriteLine("Caught Exception: " + ex.Message);
                        sr.WriteLine("Response Status Code: " + ex.StatusCode);
                        sr.WriteLine("Error Code: " + ex.ErrorCode);
                        sr.WriteLine("Request ID: " + ex.RequestId);
                    }
                }
                sr.WriteLine("Press any key to continue...");
            }
            return sb.ToString();
        }
    }
}