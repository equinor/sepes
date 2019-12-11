# SEPES #

In this document SEPES, with its different components, will be described/ documented. 

## SEPES Frontend ##

Frontend allowes users to manage 

This contains the following roles:
- Admin: Access to all resources 
- Sponsors: Responsible for sandbox management 
- Supplier: External vendors

Tech Stack: 

Maturity: 3

Protocol: https

## Azure Portal ##

Azure Portal is used by vendors to create resources for their experiment. 
Documentation can be found at microsoft homepage

Tech Stack: N/A

Maturity: 

Protocol: https

## SEPES REST API (Backend) ##

Coordinate the different services, and make actions on the behalf of the user.

Tech Stack: asp.net core, C#

Maturity:

Protocol: Uses https rest protocol and can be configured to use http. http should only be used for cases that requires it.

As an example docker compose is configured to use http, this is still safe as the docker wrapper uses https outside.

## Azure Resource Groups ##

SEPES creates resource groups for every Sandbox, enabling suppliers to create their infrastructure.

Tech Stack:

Maturity:

Protocol: https rest protocol

## Azure Networks ##

Azure network is sandbox/ firewall/ safety area/ container which a sandbox lives within.

Tech Stack: 

Protocol: https rest protocol

## MSSQL ##

Database used for persistence, that means data are saved, and retrieved from.

Tech Stack:

Maturity: 

encrypted or not?

Protocol:

## Storage Account ##

Temporary saving that is used in experiment, to mirror data sets from business storage. This is to avoid that the user gets access further into the organisation main data storage. 

Tech Stack: Firewall, private link.

Maturity:

Protocol: DON'T USE HTTP against storage account, even in tests.

## Policy store (OPA), where to find the OPA files? ##

Policy files are stored on a storage account, and is copied to SEPES backend when it is compiled. The OPA engine runs within Backend, and reads these copied files.
Configuration to restore the integrity for datas and risk profile. 

Maturity:

Protocol: Uses http, so have to be driven in the docker. Link documentation at OPA's site. OPA is driven in SEPES RestAPI.


## Azure AD ##

Used for autentication

Maturity: 

Link for Azure AD use. https://docs.microsoft.com/en-us/azure/active-directory/

Protocol: OAuth2, OpenID Connect, https

## Data Catalog ##

* *Description of implementation and technology 


Maturity:

Protocol: https


## Data Source

The data catalog provides the link to data.

Protocol: https



Maturity is based on Maufacturing Readiness Level. https://rescoll.fr/wp-content/uploads/2014/01/MRL.jpg