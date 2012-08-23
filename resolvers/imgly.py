# -*- coding: utf-8 -*-

import re

class imgly:
	regexStr = "^https?://(?:www\\.)?img\\.ly/(?:show/\\w+/)?(\\w+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://img.ly/show/full/" + match.group(1)
	
	def getLargeSize(self, match):
		return "http://img.ly/show/large/" + match.group(1)
	
	def getThumbnail(self, match):
		return "http://img.ly/show/thumb/" + match.group(1)
