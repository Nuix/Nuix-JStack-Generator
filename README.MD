Nuix JStack Generator
=====================

[![License](https://img.shields.io/badge/License-Apache%202.0-blue.svg)](http://www.apache.org/licenses/LICENSE-2.0)

**Created By:** Jason Wells

# Overview

This tool provides a simplified mechanism for generating JStack information from Nuix products.

# Getting Started

## Requirements

This tool currently only works on Windows and requires that [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472) or higher be installed.

# Usage

Begin by running `Nuix JStack Generator.exe` as **Administrator**.  The tool requires the elevated Administrator permissions to interogate the status of some processes.

Once started, specify an output directory for the JStack files to be written to and select which Nuix processes you wish to collect JStacks from.  Then you can either collect a round of JStacks immediately using the **Generate JStack** button or begin periodically collecting JStacks using the **Begin JStack Gathering** button.  Periodic JStack collection will continue until you press the **Stop JStack Gathering** button.

Generated JStack files will be created in the specified output directory with a name in the format:

```
<PROCESS NAME>_PID<PROCESS ID>_YYYYMMDD_HHMMSS_ffff_JSTACK.txt
```

For example you will get a file named like the following:

```
NUIX_APP_PID23456_20191211_152550_9116_JSTACK.txt
```

When the following is true:

- The process name is `NUIX_APP`
- The process ID is `23456`
- The date is `2019/12/11`
- The time is `15:25:50.9116`

# License

```
Copyright 2019 Nuix

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
```

# MahApps Metro

This tool uses [MahApps.Metro](https://mahapps.com/) to style the WPF interface.

```
MIT License (MIT)

Copyright (c) 2016 MahApps

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
```