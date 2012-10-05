# -*- coding: utf-8 -*-

import re

class gyazo:
	def __str__(self):
		return "Gyazo"
	
	regexStr = "^http://(?:www\\.)?gyazo\\.com/(\w+)(?:\\.png)?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getUri(self, match):
		return "http://gyazo.com/" + match.group(1) + ".png"
	
	def getFullSize(self, match):
		return self.getUri(match)
	
	def getLargeSize(self, match):
		return self.getUri(match)
	
	def getThumbnail(self, match):
		return self.getUri(match)
