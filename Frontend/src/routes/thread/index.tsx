import { Component, h } from "preact";
import { FullThread} from "../../models";
import Loading from "../../components/Loading";
import ky from "ky";
import { route } from "preact-router";
import * as style from "./style.css";
import * as S from "./thread.styled";

interface Props {
    threadId: number;
}

interface State {
    thread?: FullThread;
}

export default class ThreadPage extends Component<Props, State> {
    componentDidMount() {
        ky.get(`/api/threads/${this.props.threadId}`)
            .json<FullThread>()
            .then(thread => this.setState({ thread }));
    }

    render(props: Props, { thread }: State) {
        if (thread === undefined) return <Loading title={"Fetching threads"}/>;

        return (
            <S.Wrapper>
                <S.Title>{thread.name}</S.Title>
                <S.MemeList>
                    {thread.memes.map(m => (
                        <li style={`background-image: url("/api/file/${props.threadId}/${m.id}/snapshot")`} className={`${style.name} truncate`} key={m.id} onClick={() => route(`/${this.props.threadId}/${m.id}`)} >
                        <S.MemeOverlay>
                            <div style={"background-color: rgba(0,0,0,0.5)"}>{m.name}</div>

                        </S.MemeOverlay>
                        </li>
                    ))}
                </S.MemeList>
            </S.Wrapper>
        );
    }
}