# -*- coding: utf-8 -*-

import re

class pckles:
	regexStr = "^https?://pckles\\.com/(\w+)/(\w+)/?(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	def getFullSize(self, match):
		return "http://pckles.com/%s/%s.png" % (match.group(1), match.group(2))
	
	def getLargeSize(self, match):
		return "http://pckles.com/%s/%s.jpg" % (match.group(1), match.group(2))
	
	def getThumbnail(self, match):
		return "http://pckles.com/%s/%s.resize.jpg" % (match.group(1), match.group(2))
