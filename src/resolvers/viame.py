# -*- coding: utf-8 -*-

import re

class viame:
	def __str__(self):
		return "Via.Me"
	
	regexStr = "^https?://(?:www\\.)?via\\.me/\\-(\\w+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://via.me/-%s/thumb/r600x600" % match.group(1)
	
	def getLargeSize(self, match):
		return "http://via.me/-%s/thumb/s300x300" % match.group(1)
	
	def getThumbnail(self, match):
		return "http://via.me/-%s/thumb/s150x150" % match.group(1)
