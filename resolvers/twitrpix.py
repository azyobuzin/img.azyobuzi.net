# -*- coding: utf-8 -*-

import re

class twitrpix:
	def __str__(self):
		return "TwitrPix"
	
	regexStr = "^http://(?:www\\.)?twitrpix\\.com/(\\w+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://img.twitrpix.com/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://img.twitrpix.com/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://img.twitrpix.com/thumb/" + match.group(1)
