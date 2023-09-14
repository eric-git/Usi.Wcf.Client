using Common.ServiceClient;

namespace UsiClient.Request;

public interface IRequestProvider
{
    BulkUploadRequest GetBulkUploadRequest();
    BulkVerifyUSIRequest GetBulkVerifyUSIRequest();
    VerifyUSIRequest GetVerifyUSIRequest();
    BulkUploadRetrieveRequest GetBulkUploadRetrieveRequest();
    CreateUSIRequest GetCreateUSIRequest();
    GetNonDvsDocumentTypesRequest GetNonDvsDocumentTypesRequest();
    UpdateUSIContactDetailsRequest GetUpdateUSIContactDetailsRequest();
    UpdateUSIPersonalDetailsRequest GetUSIPersonalDetailsRequest();
    LocateUSIRequest GetLocateUSIRequest();
    GetCountriesRequest GetCountriesRequest();
}