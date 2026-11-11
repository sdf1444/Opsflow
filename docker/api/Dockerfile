FROM node:22-alpine

RUN apk add --no-cache bash ca-certificates curl icu-libs libgcc libstdc++ \
	&& curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh \
	&& chmod +x /tmp/dotnet-install.sh \
	&& /tmp/dotnet-install.sh --channel 9.0 --install-dir /usr/share/dotnet \
	&& ln -s /usr/share/dotnet/dotnet /usr/bin/dotnet \
	&& rm /tmp/dotnet-install.sh

ENV DOTNET_ROOT=/usr/share/dotnet
ENV PATH="${PATH}:/usr/share/dotnet"

WORKDIR /src

COPY backend/*.sln ./
COPY backend/ .

RUN dotnet restore
RUN dotnet build -c Debug

WORKDIR /src/OpsFlow.Api

EXPOSE 8080

CMD ["dotnet", "watch", "run", "--urls=http://0.0.0.0:8080"]
