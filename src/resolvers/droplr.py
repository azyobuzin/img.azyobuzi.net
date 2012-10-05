# -*- coding: utf-8 -*-

import re

class droplr:
	def __str__(self):
		return "Droplr"
	
	regexStr = "^https?://d\\.pr/(?:i/)?(\w+)\\+?/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getUri(self, match):
		return "http://d.pr/i/" + match.group(1) + "+"
	
	def getFullSize(self, match):
		return self.getUri(match)
	
	def getLargeSize(self, match):
		return self.getUri(match)
	
	def getThumbnail(self, match):
		return self.getUri(match)
