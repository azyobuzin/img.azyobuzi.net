# -*- coding: utf-8 -*-

import traceback
import json

error = {
	4000: "\"uri\" parameter is required.",
	4001: "Could'nt resolve the URI. Maybe not supported.",
	4002: "\"size\" parameter is invalid.",
	5000: "Could'nt resolve the URI. Maybe the URI is not found.",
	5001: "Raised unknown exception on server."
}

def handleError(status, errCode, exception):
	ret = {"code": errCode, "message": error[errCode]}
	
	if exception is not None:
		ret["exception"] = traceback.format_exc(exception)
	
	print "Status: " + status
	print "Content-Type: application/json"
	print
	print json.dumps({"error": ret})
	
	exit()
