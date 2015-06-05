# CustomProjects

This is an MultiProcess Application as it concludes producer and consumer processes. It is the 2nd project of the
course named SWE573 - Software Development Practice in Software Engineering Ms / BOUN by Mehmet Ufuk Çağlayan.

REQUIREMENTS:
Background multiple process application is defined as one main process, a number of producer processes
and a number of consumer processes. Note that the term “process” stands for an operating system process 
here, but not an operating system process thread.

1.Main Process

1.1. Opens/creates a shared log file in append mode 
1.2. Reads a configuration file to retrieve application parameters
1.3. Creates a a number of producer processes and a number of consumer processes, as specified in 
the configuration file. Producer and consumer processes will have their own code+data+stack spaces.
1.4. Starts execution of all producer processes and all consumer processes, then waits on child exit for 
all producer and consumer processes to terminate.

2.Producer Processes

2.1. Create a transaction and insert it in shared memory buffer, but wait if buffer is full.
2.2. Transaction creation involves, generating a random transaction length (upper bound?), generating 
a random encryption/decryption key, generating random data and encrypting data 
(ECB=Electronic Codebook Mode, 64 bits=8 byte blocks) by using the key and the selected 
algorithm. Transaction generation should be logged and log record message should contain 
transaction length, algorithm identifier, key, original data and encrypted data in hexadecimal 
notation. Transaction format is given in Section 4.
2.3. Since transaction creation will be very fast in this project, transaction creation time should be 
slowed down by using some random delay. Max delay is specified in the configuration file.
2.4. Execute in a loop for a specified time, as specified in the configuration file,
2.5. Log other significant events in a shared log file.

3.Consumer Processes

3.1. Remove a transaction from shared memory buffer and process it, but wait if buffer is empty
3.2. Processing the removed transaction means decrypting data in the transaction by using the key and 
the selected algorithm given in the transaction. Transaction processing should be logged and log 
record message should contain transaction length, algorithm identifier, key, encrypted data and 
decrypted data in hexadecimal notation.
3.3.  Since transaction processing will also be very fast, transaction processing time should be slowed 
down by using some random delay. Max delay is specified in the configuration file.
3.4. Execute in a loop for a specified time, as specified in the configuration file,
3.5. Log other significant events in a shared log file.

4.Shared Memory Buffer and Transaction Format

4.1. Shared memory buffer is to be accessed in a mutually exclusive manner
4.2. Shared memory buffer will have a capacity of at least 2000 transactions
4.3. Each transaction is at least 2+2+16+8=28 bytes long, where the first 2 bytes contain the length of 
the transaction, the next 2 bytes contain the encryption/decryption algorithm identifier, next 16 
bytes contain the 128 bit encryption/decryption key and next 8 or more bytes (in multiples of 8 
bytes) contain encrypted data.
4.4. Transaction length and key fields contain random bit strings in this project.

5.Logging
5.1. All critical actions of the main process and producer and consumer processes must be logged to 
the file bca-log.txt, where log records are expected to be in date and time order.
5.2. Log records are in the format yyyymmdd-hhmmss:sss dddd message
where yyyymmdd is the date, hhmmss:sss is the time in milli seconds,  dddd is the 4 digit pid of 
the process producing the log message and message is an explanatory text identifying the log 
action or significant event
5.3. Example significant events are log open, main process start, configuration file values and their 
meanings, pid’s of producer and consumer processes created by main, producer and consumer 
process starts, transactions created or removed, producer and consumer process exits, transaction 
produced by a producer in hexadecimal notation, transaction consumed by a consumer in 
hexadecimal notation, etc.
5.4. Since this is an application with no real user interface, checking log file is the only way to assure 
the application with concurrent processes is operating correctly, therefore you must be writing a 
rich set of log messages.
5.5. There is no practical upper bound on the length of the log message or the number of log records 
in the log file (of course the logical disk size is the limit). 
