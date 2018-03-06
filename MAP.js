function (doc, meta) {
    if (doc.type == "RabbitMQServer.LogInformation" && doc.dateNavigation)
        emit([doc.userToken, doc.functionName], doc.dateNavigation);
}