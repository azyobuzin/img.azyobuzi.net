# -*- coding: utf-8 -*-

import re
import MySQLdb
import urllib2
import json
from private_constant import *

class twitcasting:
	def __str__(self):
		return "TwitCasting"
	
	regexStr = "^https?://(?:www\\.)?twitcasting\\.tv/\\w+/movie/(\\d+)(?:\\?.*)?$"
	regex = re.compile(regexStr, re.IGNORECASE)
	
	last = None
	
	def getUriData(self, match):
		id = match.group(1)