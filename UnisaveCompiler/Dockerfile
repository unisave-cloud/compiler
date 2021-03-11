FROM mono:6.4.0

################################
# Unisave compiler docker file #
################################

# prepare /compiler directory contents
WORKDIR /compiler
COPY ./bin/Debug /compiler
RUN rm -rf ./unisave-framework ./compilation

# start the script runner
EXPOSE 17000
ENTRYPOINT ["mono", "--debug", "UnisaveCompiler.exe"]
CMD []
